using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace DatabaseController
{
    public static class DatabaseController
    {
        private static readonly MoneySplitDataClassesDataContext db = new MoneySplitDataClassesDataContext();
        private static readonly string NOT_IMPLEMENTED = "Function is not implemented yet.";
        private static readonly string NAME_CAN_NOT_BE_EMPTY = "Name can not be empty.";
        private static readonly string INTERNAL_ERROR = "Internal error.";
        private static readonly string TELEGRAM_ID_ALREADY_REGISTERED = "The given telegram id is already registered! Its context id is: ";
        private static readonly string CANNOT_SUBMIT_CHANGES = "Can not submit changes: ";
        private static readonly string ID_NOT_FOUND = "ID not registered in the database.";
        private static readonly string LIST_NAME_CAN_NOT_BE_EMPTY_OR_WHITE_SPACES = "The list name can not be empty or white spaces.";
        private static readonly string IBAN_CAN_NOT_BE_EMPTY_OR_WHITE_SPACES = "Iban can not be empty or white spaces.";
        private static readonly string OWNER_CF_CAN_NOT_BE_EMPTY_OR_WHITE_SPACES = "Fiscal code of the owner can not be empty or white spaces.";
        private static readonly string CAN_NOT_HAVE_BANK_ACCOUNT_WITH_MULTIPLE_VERSION = "Another list or bank account with the same name exist. Versions in bank account are not permitted.";
        private static readonly string LIST_NAME_MUST_BEGIN_WITH_CHAR = "Name of the lists can begin only with an alphabetic character (a-z, A-Z).";
        private static readonly string LIST_NOT_FOUND = "List Not Found.";
        private static readonly string OR_NOT_PERMITTED = "Otherwise you can not access it.";
        private static readonly string WHEN_RECURRENT_A_RECURRENCE_SHOULD_BE_SELECTED = "When recurrent a recurrence should be selected.";
        private static readonly string START_DATE_CAN_NOT_BE_NULL = "Start date must be selected.";
        private static readonly string ONLY_USER_CAN_SHARE_A_LIST_NOT_GROUP = "Only users can share a list, not a group.";
        private static readonly string CONTEXT_NOT_FOUND = "Context not found.";
        private static readonly string STORE_ALREADY_REGISTERED = "Error. The store is probably already registered.";
        private static readonly string ALL_FIELDS_SHOULD_NOT_BE_EMPTY = "All fields should not be empty.";


        private static int NextAvailableContextId {
            get
            {
                // call the procedure and get the only element in the query result.
                return db.GetNextAvailableIDValue().Single().Column1;
            }
        }

        private static Result DBSubmitOrFail(string errorString = null)
        {
            var result = Result.Try(() => db.SubmitChanges(System.Data.Linq.ConflictMode.FailOnFirstConflict),
                                                     (Exception exc) => errorString ?? INTERNAL_ERROR + CANNOT_SUBMIT_CHANGES + exc.ToString());
            //db.Connection.Close();
            //db.Connection.Open();
            return result;
        }

        private static int CreateNewGeneralContext()
        {
            int newId = NextAvailableContextId;

            var newGeneralContext = new GeneralContext
            {
                ContextId = newId
            };
            db.GeneralContexts.InsertOnSubmit(newGeneralContext);
            db.SubmitChanges();
            return newId;
        }

        private static Result CheckNameAndTelegramIDNotPresent(string newName, string newTelegramID = null)
        {
            newName = newName.Trim();
            if (string.IsNullOrEmpty(newName))
            {
                return Result.Fail<int>(NAME_CAN_NOT_BE_EMPTY);
            }

            if (!string.IsNullOrEmpty(newTelegramID))
            {
                var UserContext =
                    from cont in db.MoneySplitTelegramUsers
                    where cont.TelegramId == newTelegramID
                    select cont;
                if (UserContext.Count() > 0)
                {
                    return Result.Fail(TELEGRAM_ID_ALREADY_REGISTERED + UserContext.First().ContextId);
                }

                var GroupContext =
                    from cont in db.TelegramGroups
                    where cont.TelegramId == newTelegramID
                    select cont;
                if (GroupContext.Count() > 0)
                {
                    return Result.Fail(TELEGRAM_ID_ALREADY_REGISTERED + GroupContext.First().ContextId);
                }

            }
            return Result.Ok();
        }

        public static Result<int> RegisterNewUser(string newUserName, string newUserTelegramID = null)
        {
            // suppose create new user can not fail if new user name is not empty and new user telegramID is not registered yet.
            Result checkNameAndTelegramId = CheckNameAndTelegramIDNotPresent(newUserName, newUserTelegramID);
            if (checkNameAndTelegramId.IsFailure)
            {
                return Result.Fail<int>(checkNameAndTelegramId.Error);
            }

            int newId = CreateNewGeneralContext();

            var newMoneySplitUser = new MoneySplitUser
            {
                Name = newUserName,
                ContextId = newId
            };
            db.MoneySplitUsers.InsertOnSubmit(newMoneySplitUser);

            if (!string.IsNullOrEmpty(newUserTelegramID))
            {
                var newMoneySplitTelegramUser = new MoneySplitTelegramUser
                {
                    ContextId = newId,
                    TelegramId = newUserTelegramID
                };
                db.MoneySplitTelegramUsers.InsertOnSubmit(newMoneySplitTelegramUser);
            }

            return DBSubmitOrFail().Map(() => newId);
        }

        public static Result<int> RegisterNewGroup(string newGroupName, string newGroupTelegramID)
        {
            // suppose create new user can not fail if new user name is not empty and new user telegramID is not registered yet.

            Result checkNameAndTelegramId = CheckNameAndTelegramIDNotPresent(newGroupName, newGroupTelegramID);
            if (checkNameAndTelegramId.IsFailure || string.IsNullOrEmpty(newGroupTelegramID))
            {
                return Result.Fail<int>(checkNameAndTelegramId.Error);
            }

            int newId = CreateNewGeneralContext();

            var newMoneySplitGroup = new TelegramGroup
            {
                Name = newGroupName,
                TelegramId = newGroupTelegramID,
                ContextId = newId
            };
            db.TelegramGroups.InsertOnSubmit(newMoneySplitGroup);

            return DBSubmitOrFail().Map(() => newId);
        }

        public static Result GroupAddUser(string groupTelegramId, string userTelegramId, bool isAdmin = false)
        {
            var newGroupUser = new GroupUser
            {
                TelegramUserId = userTelegramId,
                GroupId = groupTelegramId,
                IsAdmin = isAdmin ? '1' : '0'
            };
            db.GroupUsers.InsertOnSubmit(newGroupUser);
            return DBSubmitOrFail();
        }

        public static Result CheckRealContextID(int contextId)
        {
            var contextUser =
                from cont in db.MoneySplitUsers
                where cont.ContextId == contextId
                select cont.ContextId;

            var contextGroup =
                from cont in db.TelegramGroups
                where cont.ContextId == contextId
                select cont.ContextId;

            return Result.Create(contextUser.Count() == 1, contextUser, ID_NOT_FOUND)
                         .OnFailureCompensate(() => Result.Create(contextGroup.Count() == 1, contextGroup, ID_NOT_FOUND));
        }

        public static Result CheckContextID(int contextId)
        {
            var context =
                from cont in db.GeneralContexts
                where cont.ContextId == contextId
                select cont.ContextId;
            return Result.Create(context.Count() == 1, context, ID_NOT_FOUND);
        }

        public static Result<int> TelegramToContextID(string telegramID)
        {
            var contextUser =
                from cont in db.MoneySplitTelegramUsers
                where cont.TelegramId == telegramID && cont.TelegramId != null
                select cont.ContextId;

            var contextGroup =
                from cont in db.TelegramGroups
                where cont.TelegramId == telegramID && cont.TelegramId != null
                select cont.ContextId;

            return Result.Create(contextUser.Count() > 0, contextUser, ID_NOT_FOUND)
                         .OnFailureCompensate(() => Result.Create(contextGroup.Count() > 0, contextGroup, ID_NOT_FOUND))
                         .Map(context => (int)context.First());
        }

        public static Result<MoneySplitCompleteUser> GetCompleteUser(int userContextID)
        {
            var user =
                from compUser in db.MoneySplitCompleteUsers
                where compUser.ContextId == userContextID
                select compUser;
            return Result.Create(user.Count() == 1, user, ID_NOT_FOUND)
                         .Map(userVal => userVal.First());
        }

        public static Result<TelegramGroup> GetCompleteGroup(int groupContextID)
        {
            var group =
                from compGroup in db.TelegramGroups
                where compGroup.ContextId == groupContextID
                select compGroup;
            return Result.Create(group.Count() == 1, group, ID_NOT_FOUND)
                         .Map(groupVal => groupVal.First());
        }

        public static bool IsGroup(int contextID)
        {
            var group =
                from groups in db.TelegramGroups
                where groups.ContextId == contextID
                select groups;

            return group.Count() == 1;
        }

        public static IEnumerable<GroupUser> GetAllMembersOfGroup(string telegramID)
        {
            if (string.IsNullOrEmpty(telegramID))
            {
                return new List<GroupUser>();
            }
            return from groupUsers in db.GroupUsers
                   where groupUsers.GroupId == telegramID
                   select groupUsers;
        }

        public static Result AddNewList(int contextID, string listName, bool isBankAccount, string IBAN = null, string ownerCF = null)
        {
            return CheckContextID(contextID).Ensure(() => !string.IsNullOrWhiteSpace(listName), LIST_NAME_CAN_NOT_BE_EMPTY_OR_WHITE_SPACES)
                                            .Ensure(() =>
                                                    {
                                                        char firstChar = listName.Trim().First();
                                                        return (firstChar <= 'z' && firstChar >= 'a') || (firstChar <= 'Z') && (firstChar >= 'A');
                                                    }, LIST_NAME_MUST_BEGIN_WITH_CHAR)
                                            .Ensure(() => IBAN == null || !string.IsNullOrWhiteSpace(IBAN), IBAN_CAN_NOT_BE_EMPTY_OR_WHITE_SPACES)
                                            .Ensure(() => ownerCF == null || !string.IsNullOrWhiteSpace(ownerCF), OWNER_CF_CAN_NOT_BE_EMPTY_OR_WHITE_SPACES)
                                            .Map(() => from list in db.GeneralLists
                                                       where list.ContextId == contextID && list.Name == listName
                                                       select list)
                                            .Map(lists => lists.Count() + 1)
                                            .Ensure(newVersion => (isBankAccount ? newVersion == 1 : true), CAN_NOT_HAVE_BANK_ACCOUNT_WITH_MULTIPLE_VERSION)
                                            .Map(newVersion => new GeneralList
                                            {
                                                Vers = newVersion,
                                                Name = listName.Trim(),
                                                ContextId = contextID,
                                                Iban = IBAN,
                                                CF_owner = ownerCF,
                                                ListType = isBankAccount ? 1 : 0
                                            })
                                            .OnSuccess(newList => db.GeneralLists.InsertOnSubmit(newList))
                                            .Map(value => DBSubmitOrFail());
        }

        public static Result<IEnumerable<GeneralList>> GetAllLists(int contextID, bool includeGeneralList = true, bool includeBankAccount = true)
        {
            var checkIDResult = CheckContextID(contextID);
            if (checkIDResult.IsFailure) { return Result.Fail<IEnumerable<GeneralList>>(checkIDResult.Error); }

            // groups in which is the user (or none if the context is a group)
            var groups =
                (from GU in db.GroupUsers
                 join MSTU in db.MoneySplitTelegramUsers on GU.TelegramUserId equals MSTU.TelegramId
                 where MSTU.ContextId == contextID
                 select GU.TelegramGroup.ContextId).Distinct();

            // shared contexts in which is the user (or none if the context is a group)
            var sharedContextsUser =
                (from sharedContext in db.SharedContextUsers
                 where sharedContext.UserId == contextID
                 select sharedContext.SharedContextId).Distinct();

            // shared context in which there is a group in which there is the user (or none if the context is a group)
            var sharedContextUserGroup =
                (from SCG in db.SharedContextGroups
                 join GU in db.GroupUsers on SCG.TelegramGroup equals GU.TelegramGroup
                 join MSTU in db.MoneySplitTelegramUsers on GU.TelegramUserId equals MSTU.TelegramId
                 where MSTU.ContextId == contextID
                 select SCG.SharedContextId).Distinct();

            var sharedContextGroup =
                (from SCG in db.SharedContextGroups
                 where SCG.TelegramGroup.ContextId == contextID
                 select SCG.SharedContextId).Distinct();

            var lists =
                (from list in db.GeneralLists
                 where list.ContextId == contextID
                    || groups.Contains(list.ContextId)
                    || sharedContextsUser.Contains(list.ContextId)
                    || sharedContextUserGroup.Contains(list.ContextId)
                    || sharedContextGroup.Contains(list.ContextId)
                 select list).Distinct()
                            .Where(list => !(list.ListType == 1 && !includeBankAccount))
                            .Where(list => !(list.ListType == 0 && !includeGeneralList));

            return Result.Ok<IEnumerable<GeneralList>>(lists);
        }

        public static Result<IEnumerable<GeneralListDouble>> GetAllListsDouble(int contextID, bool includeGeneralList = true, bool includeBankAccount = true)
        {
            return GetAllLists(contextID, includeGeneralList, includeBankAccount).Map(lists => lists.Select(list => new GeneralListDouble(list)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operatorContextID"></param>
        /// <param name="listID"></param>
        /// <param name="version">Is null if all version are requested</param>
        /// <param name="transactionType">Is null if all transaction type are requested</param>
        /// <returns></returns>
        public static Result<IEnumerable<GeneralTransactionDouble>> GetAllTransaction(int operatorContextID, int listID, int? version = null, int? transactionType = null, IEnumerable<string> hashtags = null)
        {
            var listsResult = GetAllListsDouble(operatorContextID);
            if (listsResult.IsFailure) { return Result.Fail<IEnumerable<GeneralTransactionDouble>>(listsResult.Error); }
            var allListsUser = listsResult.Value;
            var identifiedLists = allListsUser.Where(list => list.ListId == listID);
            if (identifiedLists.Count() == 0) { return Result.Fail<IEnumerable<GeneralTransactionDouble>>(LIST_NOT_FOUND + OR_NOT_PERMITTED); }
            var refList = identifiedLists.First();

            var lists = allListsUser.Where(list => list.Name == refList.Name)
                                    .Where(list => version == null ? true : list.Vers == version);
            var listsIDs = lists.Select(list => list.ListId);

            var transactions =
                from tr in db.GeneralTransactions
                where listsIDs.Contains(tr.ListId)
                select tr;
            if (hashtags != null && hashtags.Count() > 0)
            {
                transactions =
                    from tr in transactions
                    join tagTr in db.TaggedTransactions on tr.TransactionId equals tagTr.TransactionId
                    where hashtags.Contains(tagTr.TagName)
                    select tr;
            }

            transactions = transactions.Where(tr => transactionType == null ? true : tr.TransType == transactionType);
            return Result.Ok<IEnumerable<GeneralTransactionDouble>>(transactions.Select(tran => new GeneralTransactionDouble(tran)));
        }

        public static Result<int> GetNumberOfListVersions(int operatorContextID, int listID)
        {
            var listsResult = GetAllListsDouble(operatorContextID);
            var completeListRes = listsResult.Map(lists => lists.Where(list => list.ListId == listID))
                                                 .Ensure(lists => lists.Count() == 1, LIST_NOT_FOUND + OR_NOT_PERMITTED)
                                                 .Map(lists => lists.First());
            if (completeListRes.IsFailure) { return Result.Fail<int>(completeListRes.Error); }
            var completeList = completeListRes.Value;
            return listsResult.Map(lists => lists.Where(list => list.Name == completeList.Name && list.ContextId == completeList.ContextId))
                              .Map(lists => lists.Count());
        }

        public static Result<GeneralListDouble> GetListFullDetail(int operatorContextID, int listID)
        {
            return GetAllListsDouble(operatorContextID).Map(lists => lists.Where(list => list.ListId == listID))
                                                 .Ensure(lists => lists.Count() == 1, LIST_NOT_FOUND + OR_NOT_PERMITTED)
                                                 .Map(lists => lists.First());
        }

        private static bool Xor(bool a, bool b)
        {
            return (a && !b) || (!a && b);
        }

        public static Result AddNewTransaction(int operatorContextID, int listID, decimal amount, string description, int transactionType = 0, int? userAuthor = null, int? dayRecurrence = null, int? monthRecurrence = null, DateTime? startDate = null, DateTime? endDate = null, DateTime? date = null, DateTime? recurrenceTime = null)
        {

            var listRes = GetAllLists(operatorContextID).Map(lists => lists.Where(list1 => list1.ListId == listID))
                                                        .Ensure(lists => lists.Count() == 1, LIST_NOT_FOUND + OR_NOT_PERMITTED)
                                                        .Map(lists => lists.First())
                                                        .Ensure(value => (transactionType == 0 || (transactionType == 1 && (Xor(dayRecurrence == null, monthRecurrence == null)))), WHEN_RECURRENT_A_RECURRENCE_SHOULD_BE_SELECTED)
                                                        .Ensure(value => (transactionType == 0 || (transactionType == 1 && startDate != null)), START_DATE_CAN_NOT_BE_NULL);
            if (listRes.IsFailure) return listRes;

            var list = listRes.Value;
            int newAmount = (int)(amount * 100);
            GeneralTransaction newTransaction;

            if (transactionType == 0)
            {
                newTransaction = new GeneralTransaction
                {
                    Amount = newAmount,
                    Description = description,
                    TransType = transactionType,
                    Date = date ?? DateTime.Now,
                    ListId = listID,
                    UserAuthor = userAuthor
                };
                list.TotalAmount += newAmount;
            } else
            {
                newTransaction = new GeneralTransaction
                {
                    Amount = newAmount,
                    Description = description,
                    TransType = transactionType,
                    Date = date ?? DateTime.Now,
                    DayRecurrence = dayRecurrence,
                    MonthRecurrence = monthRecurrence,
                    Time = recurrenceTime,
                    StartDate = startDate,
                    EndDate = endDate,
                    ListId = listID,
                    UserAuthor = userAuthor
                };
            }
            db.GeneralTransactions.InsertOnSubmit(newTransaction);
            return DBSubmitOrFail().Map(() => InsertHashTags(newTransaction.TransactionId, description));
        }

        private static Result InsertHashTags(int transactionId, string description)
        {
            var hashtags = description.Split(' ')
                                      .Where(word => word.Length > 1 && word.StartsWith("#"))
                                      .Select(word => word.Remove(0, 1)).Distinct();
            var presentHashtags =
                from ht in db.Tags
                select ht;
            var presentHashtagsName = presentHashtags.Select(ht => ht.TagName);
            foreach (string hashtag in hashtags)
            {
                if (!presentHashtagsName.Contains(hashtag))
                {
                    var newHashTag = new Tag()
                    {
                        TagName = hashtag,
                        Usage = 1
                    };
                    db.Tags.InsertOnSubmit(newHashTag);
                } else
                {
                    var selectedHashTag = presentHashtags.Single(ht => ht.TagName == hashtag);
                    selectedHashTag.Usage += 1;
                }

                var newTaggedTransaction = new TaggedTransaction()
                {
                    TransactionId = transactionId,
                    TagName = hashtag
                };
                db.TaggedTransactions.InsertOnSubmit(newTaggedTransaction);

            }
            return DBSubmitOrFail();
        }

        public static Result<int> GetIDOfLastVersionOfList(int operatorContextID, int listID)
        {
            return GetAllLists(operatorContextID).Map(lists => lists.Where(list1 => list1.ListId == listID))
                                                 .Ensure(lists => lists.Count() == 1, LIST_NOT_FOUND + OR_NOT_PERMITTED)
                                                 .Map(lists => lists.First())
                                                 .Map(list => (from lists in db.GeneralLists
                                                               where lists.Name == list.Name && lists.ContextId == list.ContextId
                                                               select lists))
                                                 .Map(lists => lists.Where(list => list.Vers == lists.Count()))
                                                 .Ensure(lists => lists.Count() == 1, LIST_NOT_FOUND + OR_NOT_PERMITTED)
                                                 .Map(lists => lists.First())
                                                 .Map(list => (int)list.ListId);
        }

        private static Result<bool> IsContextAGroup(int contextID)
        {
            var isGroups =
                from groups in db.TelegramGroups
                where groups.ContextId == contextID
                select groups;
            if (isGroups.Count() == 1) { return Result.Ok(true); }
            var isUser =
                from user in db.MoneySplitTelegramUsers
                where user.ContextId == contextID
                select user;
            return Result.Create(isUser.Count() == 1, false, CONTEXT_NOT_FOUND);
        }

        /// <summary>
        /// NB only users can share the list, not group
        /// </summary>
        /// <param name="operatorContextID"></param>
        /// <param name="listID"></param>
        /// <returns>The id of the new shared context</returns>
        public static Result<int> ShareList(int operatorContextID, int listID, int newContextWhoWillSeeTheList)
        {
            // if list is already in a shared context it is enough to add the new context into the shared context members. otherwise a new shared context has to be created.
            var listsRes = GetAllLists(operatorContextID).Map(lists => lists.Where(list1 => list1.ListId == listID))
                                                         .Ensure(lists => lists.Count() == 1, LIST_NOT_FOUND + OR_NOT_PERMITTED)
                                                         .Map(lists => lists.First())
                                                         .Map(list => (from lists in db.GeneralLists
                                                                       where lists.Name == list.Name && lists.ContextId == list.ContextId
                                                                       select lists));
            if (listsRes.IsFailure) return Result.Fail<int>(listsRes.Error);
            // check operator context id must be a user
            var users =
                from user in db.MoneySplitUsers
                where user.ContextId == operatorContextID
                select user;
            if (users.Count() != 1) { return Result.Fail<int>(ONLY_USER_CAN_SHARE_A_LIST_NOT_GROUP); }
            // check new context if is group or not
            Result<bool> isGroupResult = IsContextAGroup(newContextWhoWillSeeTheList);

            if (isGroupResult.IsFailure) { return Result.Fail<int>(isGroupResult.Error); }
            bool isGroup = isGroupResult.Value;
            var listsSameName = listsRes.Value;

            var cont =
                from shcont in db.SharedContexts
                where shcont.ContextId == listsSameName.First().ContextId
                select shcont;

            int sharedContextId;
            if (cont.Count() == 0) // i have to create the shared context
            {
                var newSharedContext = new SharedContext
                {
                    UserCreatorId = operatorContextID,
                    ContextId = CreateNewGeneralContext()
                };
                db.SharedContexts.InsertOnSubmit(newSharedContext);

                sharedContextId = newSharedContext.ContextId;
                // put the operator in the sharedContext
                var sharConUserOperator = new SharedContextUser
                {
                    UserId = operatorContextID,
                    SharedContextId = sharedContextId
                };
                db.SharedContextUsers.InsertOnSubmit(sharConUserOperator);
            } else
            {
                sharedContextId = cont.First().ContextId;
            }
            // change the list owner to the shared context
            foreach (var list in listsSameName)
            {
                list.ContextId = sharedContextId;
            }
            // add the context to the sharedContext
            if (isGroup)
            {
                var sharConGroup = new SharedContextGroup
                {
                    GroupId = newContextWhoWillSeeTheList,
                    SharedContextId = sharedContextId
                };
                db.SharedContextGroups.InsertOnSubmit(sharConGroup);
            } else
            {
                var sharConUser = new SharedContextUser
                {
                    UserId = newContextWhoWillSeeTheList,
                    SharedContextId = sharedContextId
                };
                db.SharedContextUsers.InsertOnSubmit(sharConUser);
            }
            return DBSubmitOrFail().Map(() => sharedContextId);

        }

        public static Result RegisterNewStore(string vatAccount, string storeName, string address)
        {
            if (string.IsNullOrWhiteSpace(vatAccount) || string.IsNullOrWhiteSpace(storeName) || string.IsNullOrWhiteSpace(address))
            {
                return Result.Fail(ALL_FIELDS_SHOULD_NOT_BE_EMPTY);
            }
            vatAccount = vatAccount.Replace(" ", "");

            var stores =
                from store in db.Stores
                where store.VatAccount == vatAccount.Trim()
                select store;
            if (stores.Count() == 1)
            {
                return Result.Fail(STORE_ALREADY_REGISTERED);
            }

            var newStore = new Store
            {
                VatAccount = vatAccount.Trim(),
                StoreName = storeName.Trim(),
                Address = address.Trim()
            };
            db.Stores.InsertOnSubmit(newStore);
            return DBSubmitOrFail();
        }
    }
}
