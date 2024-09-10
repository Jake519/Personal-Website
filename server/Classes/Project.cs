using Newtonsoft.Json;

namespace server
{
    public class Project 
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Link { get; set; }
    }

    public class ProjectList
    {
        public List<Project> Projects { get; set; }
        public bool Error { get; set; }

        public ProjectList()
        {
            Projects = new();
            Error = false;
        }

        public void Append(Project project)
        {
            Projects.Add(project);
        }
    }

    public static class ProjectService
    {
        private static DateTime lastFetch = DateTime.MinValue;

        private static ProjectList projectList = new();

        public static async Task<ProjectList> GetProjects() 
        {
            if (DateTime.Now.Subtract(lastFetch).TotalMinutes > 1 || projectList.Projects.Count == 0)
            {
                ProjectList projects = new ProjectList();
                bool failedLoad = false;

                HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "Other");
                client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");

                try
                {
                    string responseBody = await client.GetStringAsync("https://api.github.com/users/Jake519/repos");

                    if (responseBody != string.Empty)
                    {
                        dynamic? deserializedResponse = JsonConvert.DeserializeObject(responseBody);

                        if (deserializedResponse != null)
                        {
                            if (deserializedResponse.Count != 0)
                            {
                                foreach (Newtonsoft.Json.Linq.JObject repo in deserializedResponse)
                                {
                                    dynamic? repoName = repo["name"];
                                    dynamic? repoDescription = repo["description"];
                                    dynamic? repoUrl = repo["html_url"];

                                    if (repoName is not null && repoUrl is not null)
                                    {
                                        string repoNameStr = repoName.ToString();
                                        repoNameStr = repoNameStr.Replace('-', ' ');

                                        Project project = new()
                                        {
                                            Name = repoNameStr,
                                            Link = repoUrl.ToString()
                                        };

                                        if (repoDescription is not null) {
                                            project.Description = repoDescription.ToString();
                                        }

                                        projects.Append(project);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                failedLoad = true;
                            }
                        }
                        else
                        {
                            failedLoad = true;
                        }
                    }
                    else
                    {
                        failedLoad = true;
                    }
                }
                catch (HttpRequestException)
                {
                    failedLoad = true;
                }

                projects.Error = failedLoad;

                lastFetch = DateTime.Now;
                projectList = projects;
                return projects;
            }
            return projectList;
        }
    }
}