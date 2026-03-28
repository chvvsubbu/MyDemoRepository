
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;

namespace SeleniumTests
{
    public class AzureDevOpsBugCreator
{
    private readonly IRestClient client;
    private readonly AzureDevOpsConfig config;

    public AzureDevOpsBugCreator(IRestClient client, AzureDevOpsConfig config)
    {
        this.client = client;
        this.config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task CreateBugAsync(string bugTitle, string assignedTo = null)
    {
        if (string.IsNullOrWhiteSpace(bugTitle))
            throw new ArgumentException("Bug title is required.", nameof(bugTitle));

        var request = BuildBugRequest(bugTitle, assignedTo ?? "default-user@example.com");
        var response = await client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            Console.WriteLine("Bug created successfully in Azure DevOps.");
        }
        else
        {
            Console.WriteLine($"Failed to create bug: {response.Content}");
        }
    }

    private RestRequest BuildBugRequest(string title, string assignedTo)
    {
        var request = new RestRequest($"{config.AzureDevOpsUrl}/{config.Project}/_apis/wit/workitems/$Bug?api-version=6.0", Method.Post);
        request.AddHeader("Content-Type", "application/json-patch+json");
        string authToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{config.PersonalAccessToken}"));
        request.AddHeader("Authorization", $"Basic {authToken}");

        var bugData = new[]
        {
            new { op = "add", path = "/fields/System.Title", value = title },
            new { op = "add", path = "/fields/System.Description", value = "Bug created automatically due to failed Selenium test." },
            new { op = "add", path = "/fields/System.AssignedTo", value = assignedTo },
            new { op = "add", path = "/fields/Microsoft.VSTS.TCM.ReproSteps", value = "See attached logs for detailed error." }
        };
        request.AddParameter("application/json-patch+json", JsonConvert.SerializeObject(bugData), ParameterType.RequestBody);

        return request;
    }
}
}
