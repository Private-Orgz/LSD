using LSD.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace LSD.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        private readonly string _uploadFolder = @"D:/Yolo/shayryar_yolo_app/shayryar_yolo/";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            try
            {
                // Validate the image file
                if (image == null || image.Length == 0)
                {
                    return BadRequest(new { message = "Please upload a valid image" });
                }

                // Read the image into a byte array
                byte[] imageBytes;
                using (var ms = new MemoryStream())
                {
                    await image.CopyToAsync(ms);
                    imageBytes = ms.ToArray();
                }

                // Convert the image byte array to Base64 string
                var base64Image = Convert.ToBase64String(imageBytes);

                var fileName = Path.GetFileName(image.FileName);
                var filePath = Path.Combine(_uploadFolder, fileName);
                // Prepare the payload
                var payload = new
                {
                    image_path = filePath
                };

                // Serialize the payload to JSON
                var jsonPayload = JsonSerializer.Serialize(payload);

                // Set up HttpClient
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Create HTTP content from JSON payload
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send POST request to the API
                    HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:5000/predict", content);

                    // Handle API response
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<List<YourApiResponseType>>(responseContent);

                        return Ok(new { message = result[0].class_names });
                    }
                    else
                    {
                        return BadRequest(new { message = "Error contacting API" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        public class YourApiResponseType
        {
            public List<string> class_names { get; set; }
            public string path { get; set; }

        }
    }
}