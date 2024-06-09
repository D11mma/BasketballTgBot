using Newtonsoft.Json;
using System.Text;
using Telegram.Bot.Types;
using RestSharp;

namespace BasketballTgBot.BasketballClients
{
    public class BasketballClients
    {
        private static string _address;
        private HttpClient _client;

        public BasketballClients()
        {
            _address = Constant.Address;
            _client = new HttpClient();
            _client.BaseAddress = new Uri(_address);
        }
        public async Task<string> GetTeamById(int id)
        {
            var response = await _client.GetAsync($"/api/BasketballTeam/GetTeamById?id={id}");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            return body;
        }
        public async Task<string> GetTeamByName(string name, long UserId)
        {

            var response = await _client.GetAsync($"/api/BasketballTeam/GetTeamByName?name={name}&UserId={UserId}");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            return body;
        }
        public async Task<string> GetLeagueByName(string name)
        {

            var response = await _client.GetAsync($"/api/BasketballTeam/GetLeagueByName?name={name}");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return body;
        }

        //Інші методи з бази даних
        public async Task InsertFavouriteTeamAsync(string NameOfTeam, long IdOfTeam)
        {
            var _restClient = new RestClient(new Uri(_address));
            NameOfTeam = NameOfTeam.Replace(" ", "%20");
            var request = new RestRequest($"/DatabaseController/SaveFavouriteTeam/?NameOfTeam={NameOfTeam}&IdOfTeam={IdOfTeam}", Method.Post);
            await _restClient.ExecuteAsync(request, Method.Post);
        }
        public async Task DeleteFavouriteTeamAsync(long IdOfTeam)
        {
            var _restClient = new RestClient(new Uri(_address));
            var request = new RestRequest($"/DatabaseController/DeleteFavouriteTeam/?IdOfTeam={IdOfTeam}", Method.Delete);
            await _restClient.ExecuteAsync(request, Method.Delete);
        }
        public async Task ChangeFavouriteTeamAsync(string NameOfTeam, long IdOfTeam)
        {
            var _restClient = new RestClient(new Uri(_address));
            var request = new RestRequest($"/DatabaseController/ChangeFavouriteTeam/?NameOfTeam={NameOfTeam}&IdOfTeam={IdOfTeam}", Method.Put);
            request.AddParameter("NameOfTeam", NameOfTeam);
            await _restClient.ExecuteAsync(request, Method.Put);
        }
        public async Task<string> GetFavouriteTeamAsync(long IdOfTeam)
        {
            var _restClient = new RestClient(new Uri(_address));
            var response = new RestRequest($"/DatabaseController/GetFavouriteTeam/?IdOfTeam={IdOfTeam}", Method.Get);
            var body = await _restClient.ExecuteAsync<string>(response, Method.Get);
            return body.Content;
        }
    }
}