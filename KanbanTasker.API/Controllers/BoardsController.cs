using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KanbanTasker.Model;
using LeaderAnalytics.AdaptiveClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KanbanTasker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardsController : ControllerBase
    {
        private IAdaptiveClient<IBoardServices> serviceClient;

        public BoardsController(IAdaptiveClient<IBoardServices> serviceClient) => this.serviceClient = serviceClient;

        [HttpGet]
        [Route("GetBoards")]
        public async Task<List<BoardDTO>> GetBoards() => serviceClient.Call(x => x.GetBoards());

        [HttpPost]
        [Route("SaveBoard")]
        public async Task<RowOpResult<BoardDTO>> SaveBoard(BoardDTO board) => serviceClient.Call(x => x.SaveBoard(board));

        [HttpPost]
        [Route("DeleteBoard")]
        public async Task<RowOpResult> DeleteBoard(int boardID) => serviceClient.Call(x => x.DeleteBoard(boardID));
    }
}