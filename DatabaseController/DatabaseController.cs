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
        static readonly MoneySplitDataClassesDataContext db = new MoneySplitDataClassesDataContext();
        private static readonly string NOT_IMPLEMENTED = "Function is not implemented yet.";
        private static readonly string NAME_CAN_NOT_BE_EMPTY = "Name can not be empty.";
        private static readonly string INTERNAL_ERROR = "Internal error.";
        private static readonly string TELEGRAM_ID_ALREADY_REGISTERED = "The given telegram id is already registered! Its context id is: ";
        private static readonly string CANNOT_SUBMIT_CHANGES = "Can not submit changes: ";

        private static int NextAvailableContextId {
            get
            {
                // call the procedure and get the only element in the query result.
                return db.GetNextAvailableIDValue().Single().Column1;
            }
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

        private static Result CheckNameAndTelegramID(string newName, string newTelegramID = null)
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
            Result checkNameAndTelegramId = CheckNameAndTelegramID(newUserName, newUserTelegramID);
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

            try
            {
                db.SubmitChanges();
            }
            catch (Exception exc)
            {
                return Result.Fail<int>(INTERNAL_ERROR + CANNOT_SUBMIT_CHANGES + exc.ToString());
            }

            return Result.Ok<int>(newId);
        }

        public static Result<int> RegisterNewGroup(string newGroupName, string newGroupTelegramID)
        {
            // suppose create new user can not fail if new user name is not empty and new user telegramID is not registered yet.

            Result checkNameAndTelegramId = CheckNameAndTelegramID(newGroupName, newGroupTelegramID);
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

            try
            {
                db.SubmitChanges();
            }
            catch (Exception exc)
            {
                return Result.Fail<int>(INTERNAL_ERROR + CANNOT_SUBMIT_CHANGES + exc.ToString());
            }

            return Result.Ok<int>(newId);
        }


    }
}
