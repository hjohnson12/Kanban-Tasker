using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model;
using System.Linq;
using KanbanTasker.Services.Database;
using LeaderAnalytics.AdaptiveClient;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using KanbanTasker.Model.Dto;

namespace KanbanTasker.Services
{
    public abstract class BaseService
    {
        protected Db db { get; set; }
        protected IServiceManifest ServiceManifest { get; set; }
        protected HttpClient httpClient { get; private set; }

        public BaseService(Db db, IServiceManifest serviceManifest)
        {
            this.db = db;
            ServiceManifest = serviceManifest;
        }

        public BaseService(Func<IEndPointConfiguration> endPointFactory)
        {
            var endPoint = endPointFactory();

            if (endPoint.EndPointType == EndPointType.HTTP)
                httpClient = new HttpClient { BaseAddress = new Uri(endPoint.ConnectionString) };
        }

        public virtual RowOpResult<BoardDto> ValidateBoard(RowOpResult<BoardDto> result)
        {
            BoardDto b = result.Entity;

            if (string.IsNullOrEmpty(b.Name))
                result.ErrorMessage = "Name is required.";
            else if (b.Name.Length > 100)
                result.ErrorMessage = "Name is too long.";
            if (string.IsNullOrEmpty(b.Notes))
                result.ErrorMessage = "Notes are required.";
            else if (b.Notes.Length > 1000)
                result.ErrorMessage = "Notes are too long.";

            // more validation here
            result.Success = string.IsNullOrEmpty(result.ErrorMessage);
            return result;
        }

        public virtual RowOpResult<TaskDto> ValidateTask(RowOpResult<TaskDto> result)
        {
            TaskDto t = result.Entity;

            // NOTE: Require Title? For now just set it to empty string; if not db will throw exception for null
            // Set the dates to null if nothing set.. otherwise sqlite insert crashes because of no value to insert
            if (string.IsNullOrEmpty(t.Title))
                t.Title = "";
            else if (t.Title.Length > 100)
                result.ErrorMessage = "Title is too long.";
            if (string.IsNullOrEmpty(t.Description))
                t.Description = "";
            if (string.IsNullOrEmpty(t.DueDate))
                t.DueDate = "";
            if (string.IsNullOrEmpty(t.FinishDate))
                t.FinishDate = "";
            if (string.IsNullOrEmpty(t.ReminderTime))
                t.ReminderTime = "";
            if (string.IsNullOrEmpty(t.StartDate))
                t.StartDate = "";


            // more validation here
            result.Success = string.IsNullOrEmpty(result.ErrorMessage);
            return result;
        }

        public virtual async Task<T> Get<T>(string url)
        {
            string json = await httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public virtual async Task<TResponse> Post<TRequest, TResponse>(string url, TRequest requestObject)
        {
            TResponse response = default(TResponse);

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                string json = JsonConvert.SerializeObject(requestObject);
                using (StringContent stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var httpResponse = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        httpResponse.EnsureSuccessStatusCode();
                        response = JsonConvert.DeserializeObject<TResponse>(await httpResponse.Content.ReadAsStringAsync());
                    }
                }
            }
            return response;
        }
    }
}
