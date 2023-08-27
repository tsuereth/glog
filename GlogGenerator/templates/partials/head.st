head(site,page) ::= <<
<!DOCTYPE HTML>

%if(strlen(site.LanguageCode))%
<html lang="%site.LanguageCode%">
%else%
<html>
%endif%


<head>
	<meta charset="utf-8">
	<title>%if(!page.HideTitle)%%page.Title;format="escapepunctuation"% - %endif%%site.Title%</title>
	<meta name="description" content="Blog about games, mostly.">

	
	<meta name="HandheldFriendly" content="True">
	<meta name="MobileOptimized" content="320">
	<meta name="viewport" content="width=device-width, initial-scale=1">

	<link rel="icon" href="%site.BaseURL%favicon.png" type="image/png">
	<link rel="alternate" title="%site.Title%" href="%site.BaseURL%index.xml" type="application/atom+xml">
	<link rel="canonical" href="%page.Permalink%">

	<link rel="stylesheet" href="%site.BaseURL%css/glog-style.css" type="text/css">
	<link rel="stylesheet" href="%site.BaseURL%css/font-awesome.min.css" type="text/css">
	<link rel="stylesheet" href="%site.BaseURL%css/jquery.fancybox.min.css" type="text/css">

	<script src="%site.BaseURL%js/jquery-3.2.1.slim.min.js" type="text/javascript"></script>
	<script src="%site.BaseURL%js/spoiler.js" type="text/javascript"></script>

	<link rel="stylesheet" href="//fonts.googleapis.com/css?family=Nunito:400,300,700" type="text/css">
</head>

>>
