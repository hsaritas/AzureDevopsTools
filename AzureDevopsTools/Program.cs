using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace AzureDevopsTools
{
    internal class Program
    {
        static string personalaccesstoken = "duev54spffquc66u7nazgt75zmx52seokpol2agdwks34wfcxaja";

        static void Main(string[] args)
        {
            //Console.WriteLine(CallAPIGet("https://azure.dstrace.com/Yazilim/DS%20GTS/_apis/build/builds?api-version=5.0"));
            //Console.WriteLine(CallAPIGet("https://azure.dstrace.com/Yazilim/_apis/projects?api-version=5.0"));
            //Console.WriteLine(CallAPIGet("https://azure.dstrace.com/Yazilim/DS%20OLT/_apis/work/teamsettings/iterations?api-version=6.0"));
            //Console.WriteLine(CallAPIGet("https://azure.dstrace.com/Yazilim/DS%20OLT/_apis/wit/classificationnodes/?api-version=6.0"));
            //Console.WriteLine(CallAPIGet("https://azure.dstrace.com/Yazilim/DS%20OLT/DS%20OLT%20Team/_apis/work/teamsettings/iterations?api-version=5.0"));
            //var result = CallAPIDelete("https://azure.dstrace.com/Yazilim/DS%20OLT/_apis/wit/classificationnodes/iterations/Iteration%20z?api-version=5.0");            
            //var result = CallAPIPost("https://azure.dstrace.com/Yazilim/DS%20OLT/DS%20OLT%20Team/_apis/work/teamsettings/iterations?api-version=5.0");
            //Console.WriteLine(CallAPIGet("https://azure.dstrace.com/Yazilim/_apis/projects/DS%20OLT/teams?api-version=5.0"));
            //CreateIteration("Iteration z", DateTime.Now.AddYears(1), DateTime.Now.AddYears(2));     
            ProjectList();
        }

        static async void CreateIteration(string iterationName, DateTime stDate, DateTime endDate)
        {
            var result = CallAPIPost("https://azure.dstrace.com/Yazilim/DS%20OLT/_apis/wit/classificationnodes/iterations?api-version=5.0",
                new { name = iterationName, attributes = new { startDate = stDate, finishDate = endDate } });
            var obj = JObject.Parse(result);
            var identifier = obj["identifier"].ToString();
            result = CallAPIPost("https://azure.dstrace.com/Yazilim/DS%20OLT/DS%20OLT%20Team/_apis/work/teamsettings/iterations?api-version=5.0",
                new { id = identifier });

        }

        static async void ProjectList()
        {

            try
            {
                Uri uri = new Uri("https://azure.dstrace.com/Yazilim");
                VssBasicCredential credentials = new VssBasicCredential("", personalaccesstoken);
                using ProjectHttpClient projectHttpClient = new ProjectHttpClient(uri, credentials);
                IEnumerable<TeamProjectReference> projects = projectHttpClient.GetProjects().Result;
                foreach (var pr in projects)
                {
                    var teams = CallAPIGet("https://azure.dstrace.com/Yazilim/_apis/projects/DS%20OLT/teams?api-version=5.0");
                }
            }
            catch (Exception)
            {

                throw;
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
                            string.Format("{0}:{1}", "", personalaccesstoken))));

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
                            string.Format("{0}:{1}", "", personalaccesstoken))));
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
                            string.Format("{0}:{1}", "", personalaccesstoken))));
                using (HttpResponseMessage response = client.DeleteAsync(url).Result)
                {
                    var ensure = response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    return ensure.StatusCode.ToString();
                }
            }
        }
    }
}
