# CMS AB Testing  & Optimizely Feature Experimentation Integration

This version of CMS AB Experimentation connects to Optimizely Feature Experimentation and creates A/B Experiment  in Feature Experimentation. Currently it supports only "Page View" metrics, when a user lands on experiment page and conversion is made, "Page View" metrics is also updated in Feature Expeimenation. Results of Metrics is displayed from Feature Experimentation into "CMS AB Expeirment" view within CMS.

Below is the sequence of events that happens in CMS and its respective effect in Feature Experiment Backend.

## Start AB Experiment in CMS
Test is created in Full Stack with Experimentation. "Page View" Event with page title is created to track Metrics. This is implemented by invoking Feature Expeirmentation Rest API

## User Lands on AB Experiment Page in CMS
When user lands on Experiment page of CMS, Feature experiment SDK is used to make decission by passing user context. User is assigned either variation A (ON = New Draft Page) or variation B (OFF = Current Published Page) version of the CMS Page. 

* Variation “On” maps to New Draft version of AB Test Page.
* Variation “Off” maps to Current Published version of AB Test Page.

## User navigates to conversion page in CMS
When same user navigates to conversion page from experimentat page, experimentation decision is made and is logged in Page View Metrics in experimentation results. Metrics Results is also visible from within CMS by viewing current ongoing experiment on the page.

## Abort AB Test (Experiment)
When Test is aborted, it is deleted from CMS and Experimentation is switched off in Optimizely Feature Experimentation.

## Publish Winner
When winner is selected, selected version of page is published in CMS and experimentation is switched off in Optimizely Feature Experimentation.
 
## Installation Steps
Required Packages in CMS content cloud for Optimizely Full Stack Integration to Work
* Research.Marketing.Experimentation latest version (Current Version 0.2.1)
* RestSharp (Version 108.0.1)
* Optimizely SDK

## Configuration Changes
Add below configuration changes in AppSettings.json in root in your Content Cloud Project

```
"full-stack": {
    "ProjectId": "21972070188",
    "APIVersion": 1,
    "EnviromentKey": "production",
    "SDKKey": "", //Get SDK key from app.optimizely.com
    "CacheInMinutes": 20,
    "RestAuthToken": "", //Get Rest API Key from app.optimizely.com
    "EventName": "page_view",
    "EventDescription": "Event to calculate page view metrics"
  }
```
## Session Cookie Required
Unique Session Cookie with name “FullStackUserGUID” is required for Full Stack User Context. Please create Unique session cookie with this name if does not exists.
Below is code to create unique session cookie in Foundation.

```
string userId = _cookieService.Get("FullStackUserGUID");
if (string.IsNullOrEmpty(userId))
	_cookieService.Set("FullStackUserGUID", Guid.NewGuid().ToString());
```

## Fields required for testing 

* Experiment Description
* Page View and Conversion Page
* Participation Percentage

![Screenshot1](images/Screenshot1.png?raw=true "Screenshot1")


## Startup Class Changes Required in Content Cloud 

```
services.AddABTesting(_configuration.GetConnectionString("EPiServerDB"));
services.Configure<FullStackSettings>(_configuration.GetSection("full-stack"));
```
 
## Experimentation Results
Experimentation Results is shown in CMS AB Test view as below

![image](https://user-images.githubusercontent.com/16465622/200676280-c89cfac6-ffed-4290-bbc4-5021ce63611e.png)

