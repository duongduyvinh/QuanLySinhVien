using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Services
{
    public class GeminiApiService
    {
        private readonly string _apiKey;
        private readonly string _endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent";
        private List<ChatMessage> _history;
        private readonly string _systemPrompt = @"Bạn là trợ lý tư vấn của trường Đại học Sư phạm Kỹ thuật TP.HCM (HCMUTE).
Nhiệm vụ của bạn là giải đáp thắc mắc về:
- Thông tin tuyển sinh, điểm chuẩn, ngành học
- Học phí, học bổng, hỗ trợ tài chính
- Ký túc xá, cơ sở vật chất
- Hoạt động sinh viên, câu lạc bộ
- Lịch học, thi cử, đăng ký môn học
- Thông tin liên hệ các phòng ban
Trả lời bằng tiếng Việt, thân thiện, ngắn gọn và chính xác.
Nếu không có thông tin cụ thể, hướng dẫn liên hệ đúng phòng ban.
Website chính thức: https://hcmute.edu.vn";

        public GeminiApiService(string apiKey)
        {
            _apiKey = apiKey;
            _history = new List<ChatMessage>();
        }

        public async Task<string> SendMessageAsync(string userMessage)
        {
            _history.Add(new ChatMessage { Role = "user", Content = userMessage, Timestamp = DateTime.Now });

            using (var client = new HttpClient())
            {
                var requestBody = BuildRequestBody();
                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var url = $"{_endpoint}?key={_apiKey}";
                var response = await client.PostAsync(url, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JObject.Parse(responseString);
                    try
                    {
                        var botReply = jsonResponse["candidates"][0]["content"]["parts"][0]["text"].ToString();
                        _history.Add(new ChatMessage { Role = "model", Content = botReply, Timestamp = DateTime.Now });
                        return botReply;
                    }
                    catch (Exception)
                    {
                        throw new Exception("Không thể parse phản hồi từ Gemini API.");
                    }
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Error: {response.StatusCode} - {errorResponse}");
                }
            }
        }

        private object BuildRequestBody()
        {
            var contents = _history.Select(msg => new
            {
                role = msg.Role,
                parts = new[] { new { text = msg.Content } }
            }).ToList();

            return new
            {
                system_instruction = new { parts = new[] { new { text = _systemPrompt } } },
                contents = contents,
                generationConfig = new { maxOutputTokens = 1024, temperature = 0.7 }
            };
        }
    }
}
