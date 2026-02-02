# Integration Guide

## Ticketing System Integration Examples

### JIRA Integration (PowerShell)

```powershell
# JIRA REST API integration with automatic screenshots
function Add-JiraTicketWithScreenshot {
    param(
        [string]$ProjectKey,
        [string]$Summary,
        [string]$Description,
        [string]$IssueType = "Bug"
    )
    
    # Configuration
    $jiraUrl = "https://your-domain.atlassian.net"
    $jiraEmail = "your-email@company.com"
    $jiraApiToken = $env:JIRA_API_TOKEN  # Store in environment variable
    
    # Create base64 auth header
    $base64Auth = [Convert]::ToBase64String(
        [Text.Encoding]::ASCII.GetBytes("${jiraEmail}:${jiraApiToken}")
    )
    
    # Create JIRA issue
    $issueData = @{
        fields = @{
            project = @{ key = $ProjectKey }
            summary = $Summary
            description = $Description
            issuetype = @{ name = $IssueType }
        }
    } | ConvertTo-Json -Depth 10
    
    $headers = @{
        "Authorization" = "Basic $base64Auth"
        "Content-Type" = "application/json"
    }
    
    $createResponse = Invoke-RestMethod `
        -Uri "$jiraUrl/rest/api/3/issue" `
        -Method Post `
        -Headers $headers `
        -Body $issueData
    
    $issueKey = $createResponse.key
    Write-Host "Created JIRA issue: $issueKey" -ForegroundColor Green
    
    # Capture screenshot
    $tempDir = Join-Path $env:TEMP "jira_screenshots"
    if (!(Test-Path $tempDir)) {
        New-Item -ItemType Directory -Path $tempDir | Out-Null
    }
    
    Write-Host "Capturing screenshot..." -ForegroundColor Yellow
    $result = & ScreenCapture.exe -m active -d $tempDir -p $issueKey
    
    if ($LASTEXITCODE -eq 0) {
        # Find the screenshot file
        $screenshot = Get-ChildItem $tempDir -Filter "${issueKey}*.png" | 
            Select-Object -First 1
        
        if ($screenshot) {
            Write-Host "Uploading screenshot to JIRA..." -ForegroundColor Yellow
            
            # Upload attachment
            $uploadHeaders = @{
                "Authorization" = "Basic $base64Auth"
                "X-Atlassian-Token" = "no-check"
            }
            
            $uploadResponse = Invoke-RestMethod `
                -Uri "$jiraUrl/rest/api/3/issue/$issueKey/attachments" `
                -Method Post `
                -Headers $uploadHeaders `
                -Form @{
                    file = Get-Item $screenshot.FullName
                }
            
            Write-Host "Screenshot attached successfully!" -ForegroundColor Green
            
            # Cleanup
            Remove-Item $screenshot.FullName -Force
        }
    }
    
    return $issueKey
}

# Usage
Add-JiraTicketWithScreenshot `
    -ProjectKey "PROJ" `
    -Summary "Login button not responding" `
    -Description "The login button does not respond when clicked" `
    -IssueType "Bug"
```

### ServiceNow Integration (C#)

```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ServiceNowIntegration
{
    private readonly string _instance;
    private readonly string _username;
    private readonly string _password;
    private readonly string _screenshotExePath;
    
    public ServiceNowIntegration(string instance, string username, string password, string exePath)
    {
        _instance = instance;
        _username = username;
        _password = password;
        _screenshotExePath = exePath;
    }
    
    public async Task<string> CreateIncidentWithScreenshot(
        string shortDescription,
        string description,
        string category = "Software",
        int impact = 3)
    {
        using var client = new HttpClient();
        
        // Setup authentication
        var authString = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{_username}:{_password}"));
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", authString);
        
        // Create incident
        var incidentData = new
        {
            short_description = shortDescription,
            description = description,
            category = category,
            impact = impact,
            urgency = 3
        };
        
        var json = JsonSerializer.Serialize(incidentData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync(
            $"https://{_instance}.service-now.com/api/now/table/incident",
            content);
        
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
        var incidentNumber = result.GetProperty("result")
                                   .GetProperty("number")
                                   .GetString();
        
        Console.WriteLine($"Created incident: {incidentNumber}");
        
        // Capture screenshot
        var screenshotPath = await CaptureScreenshotAsync(incidentNumber);
        
        if (screenshotPath != null)
        {
            // Attach to incident
            await AttachScreenshotToIncident(client, incidentNumber, screenshotPath);
            
            // Cleanup
            File.Delete(screenshotPath);
        }
        
        return incidentNumber;
    }
    
    private async Task<string?> CaptureScreenshotAsync(string incidentNumber)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "servicenow_screenshots");
        Directory.CreateDirectory(tempDir);
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _screenshotExePath,
                Arguments = $"-m active -d \"{tempDir}\" -p \"{incidentNumber}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        await process.WaitForExitAsync();
        
        if (process.ExitCode == 0)
        {
            var files = Directory.GetFiles(tempDir, $"{incidentNumber}*.png");
            return files.Length > 0 ? files[0] : null;
        }
        
        return null;
    }
    
    private async Task AttachScreenshotToIncident(
        HttpClient client,
        string incidentNumber,
        string filePath)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "file", Path.GetFileName(filePath));
        
        var response = await client.PostAsync(
            $"https://{_instance}.service-now.com/api/now/attachment/upload?" +
            $"table_name=incident&table_sys_id={incidentNumber}&file_name={Path.GetFileName(filePath)}",
            form);
        
        response.EnsureSuccessStatusCode();
        Console.WriteLine("Screenshot attached successfully");
    }
}

// Usage
var integration = new ServiceNowIntegration(
    instance: "dev12345",
    username: "admin",
    password: "password",
    exePath: @"C:\Tools\ScreenCapture.exe"
);

await integration.CreateIncidentWithScreenshot(
    shortDescription: "Application crash on save",
    description: "The application crashes when attempting to save large files",
    category: "Software",
    impact: 2
);
```

### Azure DevOps Integration (Python)

```python
import os
import subprocess
from pathlib import Path
import requests
from requests.auth import HTTPBasicAuth
import base64

class AzureDevOpsIntegration:
    def __init__(self, organization, project, pat_token, screenshot_exe):
        self.organization = organization
        self.project = project
        self.pat_token = pat_token
        self.screenshot_exe = screenshot_exe
        self.base_url = f"https://dev.azure.com/{organization}/{project}/_apis"
        
    def create_bug_with_screenshot(self, title, description, assigned_to=None):
        """Create a bug work item with automatic screenshot attachment"""
        
        # Create work item
        url = f"{self.base_url}/wit/workitems/$Bug?api-version=7.0"
        
        headers = {
            "Content-Type": "application/json-patch+json"
        }
        
        auth = HTTPBasicAuth('', self.pat_token)
        
        work_item_data = [
            {
                "op": "add",
                "path": "/fields/System.Title",
                "value": title
            },
            {
                "op": "add",
                "path": "/fields/Microsoft.VSTS.TCM.ReproSteps",
                "value": description
            }
        ]
        
        if assigned_to:
            work_item_data.append({
                "op": "add",
                "path": "/fields/System.AssignedTo",
                "value": assigned_to
            })
        
        response = requests.post(url, json=work_item_data, headers=headers, auth=auth)
        response.raise_for_status()
        
        work_item = response.json()
        work_item_id = work_item['id']
        
        print(f"Created work item: {work_item_id}")
        
        # Capture screenshot
        screenshot_path = self._capture_screenshot(work_item_id)
        
        if screenshot_path:
            # Attach screenshot
            self._attach_screenshot(work_item_id, screenshot_path, auth)
            
            # Cleanup
            os.remove(screenshot_path)
        
        return work_item_id
    
    def _capture_screenshot(self, work_item_id):
        """Capture screenshot using ScreenCapture.exe"""
        temp_dir = Path(os.getenv('TEMP')) / 'azdo_screenshots'
        temp_dir.mkdir(exist_ok=True)
        
        prefix = f"bug_{work_item_id}"
        
        result = subprocess.run([
            self.screenshot_exe,
            '-m', 'active',
            '-d', str(temp_dir),
            '-p', prefix
        ], capture_output=True)
        
        if result.returncode == 0:
            screenshots = list(temp_dir.glob(f"{prefix}*.png"))
            if screenshots:
                return str(max(screenshots, key=os.path.getctime))
        
        return None
    
    def _attach_screenshot(self, work_item_id, file_path, auth):
        """Attach screenshot to work item"""
        
        # Upload attachment
        upload_url = f"https://dev.azure.com/{self.organization}/_apis/wit/attachments?fileName={Path(file_path).name}&api-version=7.0"
        
        with open(file_path, 'rb') as f:
            file_content = f.read()
        
        headers = {
            "Content-Type": "application/octet-stream"
        }
        
        upload_response = requests.post(
            upload_url,
            data=file_content,
            headers=headers,
            auth=auth
        )
        upload_response.raise_for_status()
        
        attachment_url = upload_response.json()['url']
        
        # Link attachment to work item
        update_url = f"{self.base_url}/wit/workitems/{work_item_id}?api-version=7.0"
        
        update_data = [
            {
                "op": "add",
                "path": "/relations/-",
                "value": {
                    "rel": "AttachedFile",
                    "url": attachment_url,
                    "attributes": {
                        "comment": "Automatic screenshot capture"
                    }
                }
            }
        ]
        
        headers = {
            "Content-Type": "application/json-patch+json"
        }
        
        update_response = requests.patch(
            update_url,
            json=update_data,
            headers=headers,
            auth=auth
        )
        update_response.raise_for_status()
        
        print("Screenshot attached successfully")

# Usage
integration = AzureDevOpsIntegration(
    organization="myorg",
    project="MyProject",
    pat_token="your-pat-token-here",
    screenshot_exe=r"C:\Tools\ScreenCapture.exe"
)

work_item_id = integration.create_bug_with_screenshot(
    title="Submit button not working",
    description="When clicking the submit button, nothing happens",
    assigned_to="user@company.com"
)
```

### Zendesk Integration (Node.js)

```javascript
const axios = require('axios');
const FormData = require('form-data');
const fs = require('fs');
const { execSync } = require('child_process');
const path = require('path');
const os = require('os');

class ZendeskIntegration {
    constructor(subdomain, email, apiToken, screenshotExePath) {
        this.subdomain = subdomain;
        this.email = email;
        this.apiToken = apiToken;
        this.screenshotExePath = screenshotExePath;
        this.baseUrl = `https://${subdomain}.zendesk.com/api/v2`;
        this.auth = Buffer.from(`${email}/token:${apiToken}`).toString('base64');
    }
    
    async createTicketWithScreenshot(subject, description, priority = 'normal') {
        try {
            // Create ticket
            const ticketData = {
                ticket: {
                    subject: subject,
                    comment: {
                        body: description
                    },
                    priority: priority
                }
            };
            
            const ticketResponse = await axios.post(
                `${this.baseUrl}/tickets.json`,
                ticketData,
                {
                    headers: {
                        'Authorization': `Basic ${this.auth}`,
                        'Content-Type': 'application/json'
                    }
                }
            );
            
            const ticketId = ticketResponse.data.ticket.id;
            console.log(`Created ticket: ${ticketId}`);
            
            // Capture screenshot
            const screenshotPath = await this.captureScreenshot(ticketId);
            
            if (screenshotPath) {
                // Upload screenshot
                await this.attachScreenshot(ticketId, screenshotPath);
                
                // Cleanup
                fs.unlinkSync(screenshotPath);
            }
            
            return ticketId;
        } catch (error) {
            console.error('Error creating ticket:', error.message);
            throw error;
        }
    }
    
    async captureScreenshot(ticketId) {
        const tempDir = path.join(os.tmpdir(), 'zendesk_screenshots');
        
        if (!fs.existsSync(tempDir)) {
            fs.mkdirSync(tempDir, { recursive: true });
        }
        
        const prefix = `ticket_${ticketId}`;
        
        try {
            execSync(
                `"${this.screenshotExePath}" -m active -d "${tempDir}" -p "${prefix}"`,
                { stdio: 'inherit' }
            );
            
            const files = fs.readdirSync(tempDir)
                .filter(f => f.startsWith(prefix) && f.endsWith('.png'))
                .map(f => path.join(tempDir, f));
            
            return files.length > 0 ? files[0] : null;
        } catch (error) {
            console.error('Screenshot capture failed:', error.message);
            return null;
        }
    }
    
    async attachScreenshot(ticketId, filePath) {
        try {
            // Upload file
            const form = new FormData();
            form.append('file', fs.createReadStream(filePath));
            
            const uploadResponse = await axios.post(
                `${this.baseUrl}/uploads.json?filename=${path.basename(filePath)}`,
                form,
                {
                    headers: {
                        'Authorization': `Basic ${this.auth}`,
                        ...form.getHeaders()
                    }
                }
            );
            
            const uploadToken = uploadResponse.data.upload.token;
            
            // Add comment with attachment
            const commentData = {
                ticket: {
                    comment: {
                        body: 'Screenshot attached',
                        uploads: [uploadToken]
                    }
                }
            };
            
            await axios.put(
                `${this.baseUrl}/tickets/${ticketId}.json`,
                commentData,
                {
                    headers: {
                        'Authorization': `Basic ${this.auth}`,
                        'Content-Type': 'application/json'
                    }
                }
            );
            
            console.log('Screenshot attached successfully');
        } catch (error) {
            console.error('Error attaching screenshot:', error.message);
            throw error;
        }
    }
}

// Usage
const integration = new ZendeskIntegration(
    'yourcompany',
    'agent@company.com',
    'your-api-token',
    'C:\\Tools\\ScreenCapture.exe'
);

integration.createTicketWithScreenshot(
    'Application Error',
    'Error message appears when trying to export data',
    'high'
).then(ticketId => {
    console.log(`Ticket created successfully: ${ticketId}`);
}).catch(error => {
    console.error('Failed to create ticket:', error);
});
```

## Best Practices for Integration

### 1. Error Handling

Always check exit codes and implement fallback strategies:

```powershell
$screenshotPath = $null

# Try to capture screenshot
$result = & ScreenCapture.exe -m active -d $outputDir -p $prefix 2>&1

if ($LASTEXITCODE -eq 0) {
    $screenshotPath = Get-ChildItem $outputDir -Filter "${prefix}*.png" | 
        Select-Object -First 1 -ExpandProperty FullName
} else {
    Write-Warning "Screenshot capture failed: $result"
    Write-Warning "Continuing without screenshot..."
}

# Proceed with ticket creation
Create-Ticket -ScreenshotPath $screenshotPath
```

### 2. Timeout Handling

Set timeouts for screenshot capture:

```csharp
var process = new Process { /* ... */ };
process.Start();

if (!process.WaitForExit(10000)) // 10 second timeout
{
    process.Kill();
    _logger.LogWarning("Screenshot capture timed out");
}
```

### 3. Concurrent Captures

Use unique prefixes to avoid conflicts:

```python
import uuid

def capture_with_unique_id(ticket_id):
    unique_id = uuid.uuid4().hex[:8]
    prefix = f"{ticket_id}_{unique_id}"
    
    subprocess.run([
        'ScreenCapture.exe',
        '-m', 'active',
        '-p', prefix
    ])
```

### 4. Storage Management

Implement cleanup strategies:

```powershell
# Cleanup old screenshots (keep last 30 days)
$cutoffDate = (Get-Date).AddDays(-30)

Get-ChildItem $screenshotDir -Filter "*.png" | 
    Where-Object { $_.CreationTime -lt $cutoffDate } |
    Remove-Item -Force
```

### 5. Logging and Monitoring

Track screenshot capture metrics:

```csharp
public class ScreenshotMetrics
{
    public int TotalCaptures { get; set; }
    public int SuccessfulCaptures { get; set; }
    public int FailedCaptures { get; set; }
    public TimeSpan AverageDuration { get; set; }
    
    public void RecordCapture(bool success, TimeSpan duration)
    {
        TotalCaptures++;
        if (success) SuccessfulCaptures++; else FailedCaptures++;
        // Update average...
    }
}
```

## Testing Integration

### Unit Test Example (C#)

```csharp
[TestMethod]
public async Task CaptureScreenshot_ShouldReturnFilePath()
{
    // Arrange
    var service = new ScreenshotService(
        @"C:\Tools\ScreenCapture.exe",
        @"C:\Temp\Screenshots"
    );
    
    // Act
    var result = await service.CaptureActiveWindowAsync("TEST-001");
    
    // Assert
    Assert.IsNotNull(result);
    Assert.IsTrue(File.Exists(result));
    Assert.IsTrue(result.Contains("TEST-001"));
    
    // Cleanup
    File.Delete(result);
}
```

### Integration Test Example (PowerShell)

```powershell
Describe "ScreenCapture Integration" {
    It "Should capture active window" {
        $result = & ScreenCapture.exe -m active -d $TestDrive -p "test"
        
        $LASTEXITCODE | Should -Be 0
        
        $screenshot = Get-ChildItem $TestDrive -Filter "test*.png"
        $screenshot | Should -Not -BeNullOrEmpty
    }
    
    It "Should handle invalid window title gracefully" {
        $result = & ScreenCapture.exe -m window -w "NonExistentWindow12345" 2>&1
        
        $LASTEXITCODE | Should -Be 3
    }
}
```

## Support and Resources

- **Documentation**: See README.md for complete usage guide
- **Examples**: Check the examples directory
- **Issues**: Report issues on GitHub
- **Community**: Join our discussions

---

**Happy Integrating! 🚀**
