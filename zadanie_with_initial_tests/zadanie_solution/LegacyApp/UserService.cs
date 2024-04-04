using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsValidUserInput(firstName, lastName, email, dateOfBirth))
            {
                return false;
            }

            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);
            if (client == null)
            {
                return false;
            }

            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);

            if (!SetCreditLimit(user, client))
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool IsValidUserInput(string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            return !string.IsNullOrEmpty(firstName) &&
                   !string.IsNullOrEmpty(lastName) &&
                   email.Contains("@") &&
                   email.Contains(".") &&
                   CalculateAge(dateOfBirth) >= 21;
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            return new User
            {
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = email,
                DateOfBirth = dateOfBirth,
                Client = client
            };
        }

        private bool SetCreditLimit(User user, Client client)
        {
            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
                return true;
            }

            using (var userCreditService = new UserCreditService())
            {
                var creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);

                if (client.Type == "ImportantClient")
                {
                    creditLimit *= 2;
                }

                user.CreditLimit = creditLimit;
                user.HasCreditLimit = true;

                return creditLimit >= 500;
            }
        }
    }
}