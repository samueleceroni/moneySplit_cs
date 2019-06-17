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

        public static IEnumerable<GeneralTransaction> GetTransactions(int contextId, string listName, int? listVersion = null)
        {

            var allLists =
                from list in db.GeneralLists
                where list.ContextId == contextId && list.Name == listName
                select new { listId = list.ListId, listVersion = list.Vers};

            if (listVersion != null)
            {
                allLists = allLists.Where((list) => list.listVersion == listVersion);
            }
            var lists = allLists.Select((list) => list.listId);

            var Transactions =
                from trans in db.GeneralTransactions
                where lists.Contains(trans.ListId)
                select trans;
            return Transactions;
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
                (from GU in db.GroupUsers join MSTU in db.MoneySplitTelegramUsers on GU.TelegramUserId equals MSTU.TelegramId
                where MSTU.ContextId == contextID
                select GU.TelegramGroup.ContextId).Distinct();
            
            // shared contexts in which is the user (or none if the context is a group)
            var sharedContextsUser =
                (from sharedContext in db.SharedContextUsers
                 where sharedContext.UserId == contextID
                 select sharedContext.SharedContextId).Distinct();

            // shared context in which there is a group in which there is the user (or none if the context is a group)
            var sharedContextUserGroup =
                (from SCG in db.SharedContextGroups join GU in db.GroupUsers on SCG.TelegramGroup equals GU.TelegramGroup join MSTU in db.MoneySplitTelegramUsers on GU.TelegramUserId equals MSTU.TelegramId
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
                            .Where(list => !(list.ListType==1 && !includeBankAccount))
                            .Where(list => !(list.ListType==0 && !includeGeneralList));

            return Result.Ok<IEnumerable<GeneralList>>(lists);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operatorContextID"></param>
        /// <param name="listID"></param>
        /// <param name="version">Is null if all version are requested</param>
        /// <param name="transactionType">Is null if all transaction type are requested</param>
        /// <returns></returns>
        public static Result<IEnumerable<GeneralTransaction>> GetAllTransaction(int operatorContextID, int listID, int? version = null, int? transactionType = null)
        {
            var listsResult = GetAllLists(operatorContextID);
            if (listsResult.IsFailure) { return Result.Fail<IEnumerable<GeneralTransaction>>(listsResult.Error); }
            var allListsUser = listsResult.Value;
            var identifiedLists = allListsUser.Where(list => list.ListId == listID);
            if (identifiedLists.Count() == 0) { return Result.Fail<IEnumerable<GeneralTransaction>>(LIST_NOT_FOUND + OR_NOT_PERMITTED); }
            var refList = identifiedLists.First();

            var lists = allListsUser.Where(list => list.Name == refList.Name)
                                    .Where(list => version == null ? true : list.Vers == version);
            var listsIDs = lists.Select(list => list.ListId);

            var transactions =
                from tr in db.GeneralTransactions
                where listsIDs.Contains(tr.ListId)
                select tr;
            transactions = transactions.Where(tr => transactionType == null ? true : tr.TransType == transactionType);
            return Result.Ok<IEnumerable<GeneralTransaction>>(transactions);
        }

        public static Result<GeneralList> GetListFullDetail(int operatorContextID, int listID)
        {
            return GetAllLists(operatorContextID).Map(lists => lists.Where(list => list.ListId == listID))
                                                 .Ensure(lists => lists.Count() == 1, LIST_NOT_FOUND + OR_NOT_PERMITTED)
                                                 .Map(lists => lists.First());
        }

        private static bool Xor(bool a, bool b)
        {
            return (a && !b) || (!a && b);
        }

        public static Result AddNewTransaction(int operatorContextID, int listID, decimal amount, string description, int transactionType = 0, int? dayRecurrence = null, int? monthRecurrence = null, DateTime? time = null, DateTime? startDate = null, DateTime? endDate = null, int? userAuthor = null, DateTime ? date = null)
        {

            var listRes = GetAllLists(operatorContextID).Map(lists => lists.Where(list1 => list1.ListId == listID))
                                                        .Ensure(lists => lists.Count() == 1, LIST_NOT_FOUND + OR_NOT_PERMITTED)
                                                        .Map(lists => lists.First())
                                                        .Ensure(value => (transactionType == 0 && (dayRecurrence == null && monthRecurrence == null))
                                                                      || (transactionType == 1 && (Xor(dayRecurrence == null, monthRecurrence == null))), WHEN_RECURRENT_A_RECURRENCE_SHOULD_BE_SELECTED);
            if (listRes.IsFailure) return listRes;

            var list = listRes.Value;
            
            var newTransaction = new GeneralTransaction
            {
                Amount = amount,
                Description = description,
                TransType = transactionType,
                Date = date ?? DateTime.Now,
                DayRecurrence = dayRecurrence,
                MonthRecurrence = monthRecurrence,
                Time = time,
                StartDate = startDate,
                EndDate = endDate,
                ListId = listID,
                UserAuthor = userAuthor
            };
            
            var listToUpdate =
                (from lists in db.GeneralLists
                where lists.ListId == list.ListId
                select list).Single();
            decimal nextAmount = (decimal)amount + list.TotalAmount;
            listToUpdate.TotalAmount = nextAmount;
            Result result = DBSubmitOrFail();
            db.GeneralTransactions.InsertOnSubmit(newTransaction);
            return result.OnSuccess(() => DBSubmitOrFail());
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
                                                 .Map(list => (int) list.ListId);
        }
    }
}
