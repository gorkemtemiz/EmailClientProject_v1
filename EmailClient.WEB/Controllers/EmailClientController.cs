using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Web.Mvc;
using EmailClients.WEB.Models;
using System.Data.SqlClient;
using System.Data;
using System;
using System.Net.Mail;

namespace EmailClients.WEB.Controllers
{
    public class EmailClientController : Controller
    {
        static string connectionString = "Server = sqltest; Database = EmailClientDB; User ID = -----; Password = ---------;";
        SqlConnection conn = new SqlConnection(connectionString);
        static WebClient webclient = new WebClient();


        static EmailClient DeserializeJson(string serializedObj)
        {
            EmailClient deserializedEmailClient = JsonConvert.DeserializeObject<EmailClient>(serializedObj);
            return deserializedEmailClient;
        }

        static List<EmailClient> DeserializeJsonList(byte[] serializedObj)
        {
            List<EmailClient> deserializedEmailClients = JsonConvert.DeserializeObject<List<EmailClient>>(Encoding.GetEncoding("Windows-1254").GetString(serializedObj), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            return deserializedEmailClients;
        }

        static EmailClient GetObject(string path) // to get an EmailClient object from API with ID by URL
        {
            var response = webclient.DownloadString("http://localhost:6666/api/emailclient/GetByID/1");
            return DeserializeJson(response);
        }

        static List<EmailClient> GetAllObjects(string path) // to get a list of EmailClient objects from API
        {
            var response = webclient.DownloadData(path);
            return DeserializeJsonList(response);
        }

        public List<EmailClient> GetClientListfromDB() // to get a list of EmailClient objects from DB
        {
            List<EmailClient> emails = new List<EmailClient>();
            string commandText2 = "SELECT Name,Surname,Nickname,Email,Age FROM clients";
            conn.Open();
            SqlCommand cmd = new SqlCommand(commandText2, conn);
            SqlDataReader sqlreader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(sqlreader);

            foreach (DataRow row in dt.Rows)
            {
                var newEmail = new EmailClient();
                newEmail.Name = (string)row["Name"];
                newEmail.Surname = (string)row["Surname"];
                newEmail.Nickname = (string)row["Nickname"].ToString();
                newEmail.Email = (string)row["Email"].ToString();
                newEmail.Age = (int)row["Age"];
                emails.Add(newEmail);
            }
            conn.Close();
            return emails;
        }

        public void InsertNewUser(EmailClient emailClient) // to insert a new EmailClient object to DB
        {
            if (emailClient.Name.Length <= 30 && emailClient.Surname.Length <= 30 && emailClient.Nickname.Length <= 30 && emailClient.Email.Length <= 40 && emailClient.Age.GetType() == typeof(Int32))
            {
                string commandText =
                    "BEGIN IF NOT EXISTS(SELECT * FROM clients WHERE Nickname = '" + emailClient.Nickname + "') BEGIN " +
                    " INSERT INTO clients(Name,Surname,Nickname,Email,Age) " +
                    "VALUES('" + emailClient.Name + "','" + emailClient.Surname +
                    "','" + emailClient.Nickname + "','" + emailClient.Email + "'," + emailClient.Age + ") END END";

                SqlCommand command = new SqlCommand(commandText, conn);
                command.Connection.Open();
                command.ExecuteNonQuery();
                command.Connection.Close();
            }
        }

        public ActionResult GetClientListfromController()
        {

            return View();
        }

        public ActionResult Index()
        {
            var emailClients = GetClientListfromDB();
            return View(emailClients);
        }

        [HttpPost]
        public ActionResult SendMethod(string name, string surName, string nickName, string eMail, string age)
        {
            string commandText = "SELECT Name,Surname,Nickname,Email,Age FROM clients WHERE Nickname = @nickname";
            SqlCommand command = new SqlCommand(commandText, conn);
            SmtpClient smtpServer = new SmtpClient("write some number here", 666)
            {
                Credentials = new System.Net.NetworkCredential("", ""),
                EnableSsl = false
            };
            EmailClient newEmailClient = new EmailClient
            {
                Name = name,
                Surname = surName,
                Nickname = nickName,
                Email = eMail,
                Age = Int32.Parse(age)
            };

            InsertNewUser(newEmailClient);

            if (ModelState.IsValid)
            {
                command.Parameters.Add("@nickname", SqlDbType.NVarChar, nickName.Length).Value = nickName;
                command.Connection.Open();
                SqlDataReader sqlDataReader = command.ExecuteReader();

                // e-Mail sending section
                while (sqlDataReader.Read())
                {
                    MailMessage mailMessage = new MailMessage("gorkem.temiz@yemeksepeti.com",
                                                               sqlDataReader["Email"].ToString())
                            {
                             Subject = "uygulama test maili",
                                Body = sqlDataReader["Name"] + "\t"
                                     + sqlDataReader["Surname"] + "\t"
                                     + sqlDataReader["Age"].ToString() + "\t"
                                     + sqlDataReader["Nickname"] + "\t"
                            };
                    smtpServer.Send(mailMessage);
                }
            }
            return RedirectToAction("Index");
        }
    }
}