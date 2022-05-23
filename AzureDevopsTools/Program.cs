using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevopsTools
{
    internal class Program
    {
        static string PERSONAL_ACCESS_TOKEN = "duev54spffquc66u7nazgt75zmx52seokpol2agdwks34wfcxaja";
        static string BASE_URL = "https://azure.dstrace.com/Yazilim";
        
        static void Main(string[] args)
        {
            //Console.WriteLine(CallAPIGet($"{BASE_URL}/_apis/projects?api-version=5.0"));
            //Console.WriteLine(CallAPIGet($"{BASE_URL}/DS%20OLT/_apis/work/teamsettings/iterations?api-version=6.0"));
            //Console.WriteLine(CallAPIGet($"{BASE_URL}/DS%20OLT/_apis/wit/classificationnodes/?api-version=6.0"));
            //Console.WriteLine(CallAPIGet($"{BASE_URL}/DS%20OLT/DS%20OLT%20Team/_apis/work/teamsettings/iterations?api-version=5.0"));
            //Console.WriteLine(CallAPIGet($"{BASE_URL}/_apis/projects/DS OLT/teams?api-version=5.0"));
            //CallAPIDelete($"{BASE_URL}/DS%20OLT/_apis/wit/classificationnodes/iterations/Iteration%20z?api-version=5.0");

            //CreateIteration("Iteration z", DateTime.Now.AddYears(1), DateTime.Now.AddYears(2));
            AddIterationToAllProjects(
                new AzureIteration() 
                    { 
                        Name="Iteration 19", 
                        StartDate=DateTime.ParseExact("23/05/2022", "dd/MM/yyyy", null),
                        EndDate = DateTime.ParseExact("05/06/2022", "dd/MM/yyyy", null),
                    });
        }

        static void CreateIteration(string iterationName, string projectName, string teamName, DateTime stDate, DateTime endDate)
        {
            try
            {
                var result = CallAPIPost($"{BASE_URL}/{projectName}/_apis/wit/classificationnodes/iterations?api-version=5.0",
            new { name = iterationName, attributes = new { startDate = stDate, finishDate = endDate } });
                var obj = JObject.Parse(result);
                var identifier = obj["identifier"].ToString();
                result = CallAPIPost($"{BASE_URL}/{projectName}/{teamName}/_apis/work/teamsettings/iterations?api-version=5.0",
                    new { id = identifier });
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }

        static async void DeleteIteration(string iterationName, string projectName)
        {
            CallAPIDelete($"{BASE_URL}/{projectName}/_apis/wit/classificationnodes/iterations/{iterationName}?api-version=5.0");
        }

        static async Task<List<AzureObject>> GetTeamsAsync(string projectName)
        {
            var teamsStr = CallAPIGet($"{BASE_URL}/_apis/projects/{projectName}/teams?api-version=5.0");
            var teamsObj = JObject.Parse(teamsStr);
            var teamsArr = JArray.Parse(teamsObj["value"].ToString());
            return teamsArr.ToObject<List<AzureObject>>();
        }

        static async Task<List<AzureObject>> GetIterationsAsync(string projectName, string teamName)
        {
            var iterationsStr = CallAPIGet($"{BASE_URL}/{projectName}/{teamName}/_apis/work/teamsettings/iterations?api-version=5.0");
            var iterationsObj = JObject.Parse(iterationsStr);
            var iterationsArr = JArray.Parse(iterationsObj["value"].ToString());
            return iterationsArr.ToObject<List<AzureObject>>();
        }

        static async void AddIterationToAllProjects(AzureIteration iteration)
        {
            try
            {
                Uri uri = new Uri(BASE_URL);
                VssBasicCredential credentials = new VssBasicCredential("", PERSONAL_ACCESS_TOKEN);
                using ProjectHttpClient projectHttpClient = new ProjectHttpClient(uri, credentials);
                IEnumerable<TeamProjectReference> projects = projectHttpClient.GetProjects().Result;
                foreach (var project in projects)
                {

                    var teamsArr = await GetTeamsAsync(project.Name);
                    foreach (var team in teamsArr) 
                    {
                        var iterations = await GetIterationsAsync(project.Name, team.Name.ToString());
                        if (iterations.Any(x => x.Name == iteration.Name) == false)
                            CreateIteration(iteration.Name, project.Name, team.Name, iteration.StartDate, iteration.EndDate);
                        else
                            Console.WriteLine($"Iteration Exists! {project.Name}, {iteration.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static string CallAPIGet(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", PERSONAL_ACCESS_TOKEN))));

                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    var ensure = response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    var parsed = JToken.Parse(responseBody);
                    return parsed.ToString(Newtonsoft.Json.Formatting.Indented);
                }
            }
        }

        public static string CallAPIPost(string url, object model)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", PERSONAL_ACCESS_TOKEN))));
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                using (HttpResponseMessage response = client.PostAsync(url, content).Result)
                {
                    var ensure = response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    var parsed = JToken.Parse(responseBody);
                    return parsed.ToString(Newtonsoft.Json.Formatting.Indented);
                }
            }
        }

        public static string CallAPIDelete(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", PERSONAL_ACCESS_TOKEN))));
                using (HttpResponseMessage response = client.DeleteAsync(url).Result)
                {
                    var ensure = response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    return ensure.StatusCode.ToString();
                }
            }
        }
    }

    public class AzureObject 
    {
        public string Name { get; set; }
    }

    public class AzureIteration
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
