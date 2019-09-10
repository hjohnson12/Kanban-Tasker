using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KanbanTasker.Model;
using LeaderAnalytics.AdaptiveClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
namespace KanbanTasker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private IAdaptiveClient<ITaskServices> serviceClient;

        public TasksController(IAdaptiveClient<ITaskServices> serviceClient) => this.serviceClient = serviceClient;

        [HttpGet]
        [Route("GetTasks")]
        public async Task<List<TaskDTO>> GetTasks() => serviceClient.Call(x => x.GetTasks());

        [HttpPost]
        [Route("SaveTask")]
        public async Task<RowOpResult<TaskDTO>> SaveTask(TaskDTO task) => serviceClient.Call(x => x.SaveTask(task));

        [HttpPost]
        [Route("DeleteTask")]
        public async Task<RowOpResult> DeleteTask(int taskID) => serviceClient.Call(x => x.DeleteTask(taskID));

        [HttpPost]
        [Route("UpdateColumnData")]
        public async Task UpdateColumnData(TaskDTO task) => serviceClient.Call(x => x.UpdateColumnData(task));

        [HttpPost]
        [Route("UpdateCardIndex")]
        public async Task UpdateCardIndex(JObject data) => serviceClient.Call(x => x.UpdateCardIndex(data["id"].ToObject<int>(), data["currentCardIndex"].ToObject<int>()));
    }
}