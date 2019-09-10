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

        public virtual RowOpResult<BoardDTO> ValidateBoard(RowOpResult<BoardDTO> result)
        {
            BoardDTO b = result.Entity;

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

        public virtual RowOpResult<TaskDTO> ValidateTask(RowOpResult<TaskDTO> result)
        {
            TaskDTO t = result.Entity;

            if (string.IsNullOrEmpty(t.Title))
                result.ErrorMessage = "Title is required.";
            else if (t.Title.Length > 100)
                result.ErrorMessage = "Title is too long.";

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
