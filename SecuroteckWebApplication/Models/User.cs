using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml;

namespace SecuroteckWebApplication.Models
{
    public class User
    {
        [Key]
        public Guid ApiKey { get; set; }
        public string UserName { get; set; }
        public virtual ICollection<Log> Logs { get; set; }
        public User()
        {
            Logs = new Collection<Log>();
        }
    }

    public class Log
    {
        [Key]
        public Guid LogId { get; set; }
        public string LogString { get; set; }
        public DateTime LogDateTime { get; set; }
        public Log()
        {

        }
        public Log(string pLogString)
        {
            LogId = Guid.NewGuid();
            LogString = pLogString;
            LogDateTime = DateTime.Now;
        }
    }

    public class UserDatabaseAccess
    {
        public static User New(string pUserName)
        {
            using (var db = new UserContext())
            {
                User user = new User();
                user.UserName = pUserName;
                user.ApiKey = Guid.NewGuid();
                db.Users.Add(user);
                db.SaveChanges();
                return user;
            }
        }
       
        public static void NewLog(string pLogString, User user)
        {
            using (var db = new UserContext())
            {
                Log log = new Log(pLogString);
                db.Logs.Add(log);
                var entry = db.Users.SingleOrDefault(b => b.ApiKey == user.ApiKey);
                entry.Logs.Add(log);
                db.SaveChanges();
            }
        }
        public static bool CheckUser(string pUserName)
        {
            using (var db = new UserContext())
            {
                var query = from u in db.Users where 
                            u.UserName == pUserName select u;
                foreach(var result in query)
                {
                    if (result.UserName == pUserName)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public static bool CheckUser(Guid pApiKey, string pUserName)
        {
            using (var db = new UserContext())
            {
                var query = from user in db.Users
                            where user.UserName == pUserName
                            select user;
                foreach (var result in query)
                {
                    if (result.ApiKey == pApiKey)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public static User CheckUser(Guid pApiKey)
        {
            using (var db = new UserContext())
            {
                var query = from user in db.Users
                            where user.ApiKey == pApiKey
                            select user;
                foreach (var result in query)
                {
                    User user = result;
                    return user;
                }
                return null;
            }
        }
        public static void DeleteUser(Guid pApiKey)
        {
            using (var db = new UserContext())
            {
                var query = from user in db.Users
                            where user.ApiKey == pApiKey
                            select user;
                foreach (var result in query)
                {
                    if (result.Logs != null)
                    {
                        foreach(Log log in result.Logs.ToList())
                        {
                            db.Logs.Remove(log);
                        }
                    }
                    db.Users.Remove(result);
                }
                db.SaveChanges();
            }
        }
    }
}