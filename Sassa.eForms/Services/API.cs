using Sassa.eForms.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sassa.eForms.Services
{
    public class API
    {
        private readonly HttpClient client;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public API(HttpClient _client)
        {
            client = _client;
            jsonSerializerOptions = new JsonSerializerOptions() { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true };
        }

        public async Task<T> GetVodacomResult<T>(string apicall)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress + apicall);
            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            return default(T);

        }

        public async Task<SMSResponse> PostSMS(string apicall, SMSRequest item)
        {
            var serialized = JsonSerializer.Serialize(item);
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apicall)
            {
                Content = new StringContent(serialized, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                //Log error 
            }
            return await JsonSerializer.DeserializeAsync<SMSResponse>(await response.Content.ReadAsStreamAsync());

        }
        public async Task<T> GetResult<T>(string apicall)
        {
            //try
            //{
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, apicall);
            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return default(T);
                }
            }
            //return await response.Content.ReadAsAsync<T>().ConfigureAwait(false);

            try
            {
                if (response.Content.Headers.ContentType != null && response.Content.Headers.ContentType.MediaType.Contains("json", StringComparison.InvariantCultureIgnoreCase))
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        if (responseStream.Length == 0)
                            throw new JsonException("Empty stream");

                        return await JsonSerializer.DeserializeAsync<T>(responseStream, jsonSerializerOptions);
                    }
                else return await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync());
            }
            catch (JsonException)
            {
                return await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync());
            }
            //}
            //catch (Exception ex)
            //{
            //    //Logger.LogError(ex, "Exception in API::GET. Data: {0}", apicall);
            //    //throw;
            //}
        }

        public async Task<T> PostRequest<T>(string apicall, T item)
        {

            try
            {
                var serialized = JsonSerializer.Serialize(item);
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apicall)
                {
                    Content = new StringContent(serialized, Encoding.UTF8, "application/json")
                };

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    //Log error 
                }
                return await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync());
            }
            catch
            {
                //Logger.LogError(ex, "Exception in API::POST. Data: {0}", apicall);
                throw;
            }
        }

        public async Task<SassaUser> PostUser<T>(string apicall, T item)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(item), Encoding.UTF8, "application/json");
            SassaUser user;

            using (var response = await client.PostAsync(client.BaseAddress + apicall, content))
            {
                var apiResponse = response.Content.ReadAsStreamAsync().Result;
                //receivedReservation = JsonConvert.DeserializeObject<Reservation>(apiResponse);
                try
                {
                    user = await JsonSerializer.DeserializeAsync<SassaUser>(apiResponse);
                }
                catch 
                {
                    throw;
                }
            }
            return user;
        }
        //new post
        //var student = new Student() { Name = "Steve" };

        //var postTask = client.PostAsJsonAsync<Student>("student", student);
        //postTask.Wait();

        //    var result = postTask.Result;
        //    if (result.IsSuccessStatusCode)
        //    {

        //        var readTask = result.Content.ReadAsAsync<Student>();
        //readTask.Wait();

        //        var insertedStudent = readTask.Result;

        //Console.WriteLine("Student {0} inserted with id: {1}", insertedStudent.Name, insertedStudent.Id);
        //    }
        //    else
        //    {
        //        Console.WriteLine(result.StatusCode);
        //    }
        //}
        public async Task PutRequest<T>(string apicall, T item)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, apicall)
                {
                    Content = new StringContent(JsonSerializer.Serialize(item), Encoding.UTF8, "application/json")
                };
                var response = await client.SendAsync(request).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    //Log error 
                }
                return;
            }
            catch
            {
                //Logger.LogError(ex, "Exception in API::PUT. Data: {0}", apicall);
                throw;
            }
        }

        public async Task PutRequest(string apicall)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, apicall);
                var response = await client.SendAsync(request).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    //Log error 
                }
                return;
            }
            catch
            {
                //Logger.LogError(ex, "Exception in API::PUT. Data: {0}", apicall);
                throw;
            }
        }


        public async Task Delete(string apicall)
        {

            var response = await client.DeleteAsync(apicall).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                Console.Write("Success");
            }
            else
            {
                Console.Write("Error");
            }

        }
    }
}

