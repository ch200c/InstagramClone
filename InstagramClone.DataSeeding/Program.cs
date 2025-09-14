using System.Net.Http.Json;

var imageUrls = new string[]
{
    "https://ids.si.edu/ids/deliveryService?id=CHSDM-22964_02-000003&max_w=200",
    "https://ids.si.edu/ids/deliveryService?id=NPG-NPG_93_117RuthM-000001&max_w=200",
    "https://ids.si.edu/ids/deliveryService?id=CHSDM-23496_02-000001&max_w=200",
    "https://ids.si.edu/ids/deliveryService?id=CHSDM-23288_01&max_w=200",
    "https://ids.si.edu/ids/deliveryService?id=CHSDM-23560_02-000001&max_w=200"
};

var userInfoList = new List<UserInfo>();

var host = "localhost:7214";
using HttpClient internalClient = new()
{
    BaseAddress = new Uri($"https://{host}")
};

using HttpClient imageClient = new();

await GenerateUsersAndImagesAsync();
await GenerateUserFollowingsAsync();

async Task GenerateUsersAndImagesAsync()
{
    var userCount = Random.Shared.Next(3, 5);

    for (var i = 0; i < userCount; i++)
    {
        var email = $"user{i}@test.com";
        var userId = await RegisterAndUploadUserImagesAsync(email);

        userInfoList.Add(new UserInfo(userId, email));
    }
}

async Task GenerateUserFollowingsAsync()
{
    for (var i = 0; i < userInfoList.Count; i++)
    {
        var maxFollowedUsersCount = Random.Shared.Next(1, userInfoList.Count - 1);
        var count = 0;

        await LoginUserAsync(userInfoList[i].Email);

        for (var j = 0; j < userInfoList.Count; j++)
        {
            if (i == j)
            {
                continue;
            }

            if (count >= maxFollowedUsersCount)
            {
                break;
            }

            await FollowUserAsync(userInfoList[j].UserId);
            Console.WriteLine($"{userInfoList[i].Email} now follows {userInfoList[j].Email}");
            count++;
        }

        await LogoutUserAsync();
    }
}

async Task<string> RegisterAndUploadUserImagesAsync(string email)
{
    await RegisterUserAsync(email);
    Console.WriteLine($"Registered {email}");

    await LoginUserAsync(email);
    Console.WriteLine($"Logged in {email}");

    var userId = await GetUserIdAsync();

    var i = 1;
    var imageCount = Random.Shared.Next(1, 10);

    foreach (var url in GetImageUrls(imageCount))
    {
        await UploadImageAsync($"title {i}", url);
        Console.WriteLine($"Uploaded image {i}/{imageCount}");
        i++;
    }

    await LogoutUserAsync();
    Console.WriteLine($"Logged out {email}");

    return userId;
}

async Task<string> GetUserIdAsync()
{
    using var response = await internalClient.GetAsync("/userId");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
}

async Task FollowUserAsync(string userId)
{
    using var response = await internalClient.PostAsJsonAsync(
        "/api/userFollowing", new { userId = userId });
    response.EnsureSuccessStatusCode();
}

async Task RegisterUserAsync(string email)
{
    using var response = await internalClient.PostAsJsonAsync(
        "/register", new { email = email, password = "Abc123!" });
    response.EnsureSuccessStatusCode();
}

async Task LoginUserAsync(string email)
{
    using var response = await internalClient.PostAsJsonAsync(
        "/login?useCookies=true", new { email = email, password = "Abc123!" });
    response.EnsureSuccessStatusCode();
}

async Task LogoutUserAsync()
{
    using var response = await internalClient.PostAsync("/logout", null);
    response.EnsureSuccessStatusCode();
}

async Task UploadImageAsync(string title, string imageUrl)
{
    var fileName = await DownloadImageAsync(imageUrl);

    Console.WriteLine($"Uploading {fileName}");

    using var response = await internalClient.PostAsJsonAsync(
        "/api/media", new { localPath = fileName, title = title });
    response.EnsureSuccessStatusCode();
}

async Task<string> DownloadImageAsync(string imageUrl)
{
    using var response = await imageClient.GetAsync(imageUrl);
    response.EnsureSuccessStatusCode();

    var fileName = Path.GetTempFileName();
    fileName = Path.ChangeExtension(fileName, "jpg");

    using var stream = await response.Content.ReadAsStreamAsync();
    using var destination = File.OpenWrite(fileName);
    await stream.CopyToAsync(destination);

    return fileName;
}

IEnumerable<string> GetImageUrls(int count)
{
    var i = 0;
    while (i < count)
    {
        foreach (var url in imageUrls)
        {
            yield return url;
            i++;
            if (i >= count)
            {
                break;
            }
        }
    }
}

record UserInfo(string UserId, string Email);