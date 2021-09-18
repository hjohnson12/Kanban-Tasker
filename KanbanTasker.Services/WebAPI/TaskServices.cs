using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KanbanTasker.Model;
using System.Linq;
using KanbanTasker.Services.Database;
using LeaderAnalytics.AdaptiveClient;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using KanbanTasker.Model.Services;
using KanbanTasker.Model.Dto;

namespace KanbanTasker.Services.WebAPI
{
    public class TaskServices : BaseService, ITaskServices
    {
        public TaskServices(Func<IEndPointConfiguration> endPointFactory) : base(endPointFactory)  { }

        public List<TaskDTO> GetTasks() => Task.Run(() => Get<List<TaskDTO>>("Tasks/GetTasks")).Result;

        public RowOpResult<TaskDTO> SaveTask(TaskDTO task) => Task.Run(() => Post<TaskDTO, RowOpResult<TaskDTO>>("Tasks/SaveTask", task)).Result;

        public RowOpResult DeleteTask(int id) => Task.Run(() => Post<int, RowOpResult>("Tasks/DeleteTask", id)).Result;

        public void UpdateColumnData(TaskDTO task)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "Tasks/UpdateColumnData"))
            {
                string json = JsonConvert.SerializeObject(task);
                using (StringContent stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var httpResponse = httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result)
                    {
                        httpResponse.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        public void UpdateCardIndex(int iD, int currentCardIndex)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "Tasks/UpdateCardIndex"))
            {
                string json = JsonConvert.SerializeObject(new { id =iD, currentCardIndex = currentCardIndex });
                using (StringContent stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var httpResponse = httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result)
                    {
                        httpResponse.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        public void UpdateColumnName(int iD, string newColName)
        {
            throw new NotImplementedException();
        }
    }
}