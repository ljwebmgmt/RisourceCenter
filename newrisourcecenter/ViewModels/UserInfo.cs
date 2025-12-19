using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace newrisourcecenter.ViewModels
{
    public class UserInfo
    {
        public string UserName { get; set; }

        public string Title { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public string Company { get; set; }

        public string CostCenterAccount { get; set; }

        public string Department { get; set; }

        public string Facility { get; set; }

        public string UnitSystem { get; set; }

        public string DefaultLanguage { get; set; }

        public string DefaultCurrency { get; set; }

        public string AddressLineOne { get; set; }

        public string AddressLineTwo { get; set; }

        public string AddressLineThree { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public string Country { get; set; }

        public string PhoneNumber1 { get; set; }

        public string PhoneNumber2 { get; set; }

        public string PhoneNumber3 { get; set; }

        public string PhoneNumber4 { get; set; }

        public string FaxNumber { get; set; }

        public string SecretQuestion { get; set; }

        public string SecretAnswer { get; set; }

        public string LDAPName { get; set; }

        public string PSContactID { get; set; }

        public string AccountStatus { get; set; }

        public string NickName { get; set; }

        public string SSOToken { get; set; }

        public string CustomFieldOne { get; set; }

        public string CustomFieldTwo { get; set; }

        public string CustomFieldThree { get; set; }

        public string CustomFieldFour { get; set; }

        public string CustomFieldFive { get; set; }

        public string ChangePassword { get; set; }

        public string LoyaltyProgramID { get; set; }

        public string VATNumber { get; set; }
    }

    public class UserInfoMap : ClassMap<UserInfo>
    {
        public UserInfoMap()
        {
            Map(x => x.UserName).Index(0).Name("UserName");

            Map(x => x.Title).Index(1).Name("Title");

            Map(x => x.FirstName).Index(2).Name("First Name");

            Map(x => x.MiddleName).Index(3).Name("Middle Name");

            Map(x => x.LastName).Index(4).Name("Last Name");

            Map(x => x.Password).Index(5).Name("Password");

            Map(x => x.Email).Index(6).Name("Email");

            Map(x => x.Company).Index(7).Name("Company");

            Map(x => x.CostCenterAccount).Index(8).Name("Cost Center Account Number");

            Map(x => x.Department).Index(9).Name("Department");

            Map(x => x.Facility).Index(10).Name("Facility");

            Map(x => x.UnitSystem).Index(11).Name("Unit System");

            Map(x => x.DefaultLanguage).Index(12).Name("Default Language");

            Map(x => x.DefaultCurrency).Index(13).Name("Default Currency");

            Map(x => x.AddressLineOne).Index(14).Name("Address Line One");

            Map(x => x.AddressLineTwo).Index(15).Name("Address Line Two");

            Map(x => x.AddressLineThree).Index(16).Name("Address Line Three");

            Map(x => x.City).Index(17).Name("City");

            Map(x => x.State).Index(18).Name("State / Province / Region");

            Map(x => x.Zip).Index(19).Name("Zip / PostalCode");

            Map(x => x.Country).Index(20).Name("Country");

            Map(x => x.PhoneNumber1).Index(21).Name("Phone Number 1");

            Map(x => x.PhoneNumber2).Index(22).Name("Phone Number 2");

            Map(x => x.PhoneNumber3).Index(23).Name("Phone Number 3");

            Map(x => x.PhoneNumber4).Index(24).Name("Phone Number 4");

            Map(x => x.FaxNumber).Index(25).Name("Fax Number");

            Map(x => x.SecretQuestion).Index(26).Name("Secret Question");

            Map(x => x.SecretAnswer).Index(27).Name("Secret Answer");

            Map(x => x.LDAPName).Index(28).Name("LDAP Name");

            Map(x => x.PSContactID).Index(29).Name("PSContactID");

            Map(x => x.AccountStatus).Index(30).Name("Account Status");

            Map(x => x.NickName).Index(31).Name("Nickname");

            Map(x => x.SSOToken).Index(32).Name("SSOToken");

            Map(x => x.CustomFieldOne).Index(33).Name("CustomFieldOne");

            Map(x => x.CustomFieldTwo).Index(34).Name("CustomFieldTwo");

            Map(x => x.CustomFieldThree).Index(35).Name("CustomFieldThree");

            Map(x => x.CustomFieldFour).Index(36).Name("CustomFieldFour");

            Map(x => x.CustomFieldFive).Index(37).Name("CustomFieldFive");

            Map(x => x.ChangePassword).Index(38).Name("Change Password");

            Map(x => x.LoyaltyProgramID).Index(39).Name("LoyaltyProgramID");

            Map(x => x.VATNumber).Index(40).Name("VATNumber");

        }
    }

    public class UserExport
    {
        public int userID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string companyName { get; set; }
        public string companyLocation { get; set; }
        public string status { get; set; }
        public string lastLogin { get; set; }
    }
}