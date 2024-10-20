rss(site,page) ::= <<
<rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">
  <channel>
    <title>Glog</title>
    <link>%page.Permalink%</link>
    <language>en-us</language>
    <author>TS</author>

    <updated>%page.Date;format="iso8601"%</updated>

%page.HistoryPosts :{postPage |
    <item>
      <title>%postPage.Title;format="escapeforrssxml"%</title>
      <link>%postPage.Permalink%</link>
      <pubDate>%postPage.Date;format="iso8601"%</pubDate>
      <author>TS</author>
      <guid isPermalink="true">%postPage.Permalink%</guid>
%postPage.Categories :{category |
      <category>%category.Name;format="escapeforrssxml"%</category>
}%
      <description>
&lt;p&gt;&lt;i&gt;Posted%if(postPage.Categories)% in %postPage.Categories :{category |&lt;a href="%site.BaseURL%category/%category.Name;format="urlize"%"&gt;%category.Name;format="escapeforrssxml"%&lt;/a&gt;};separator=", "%%endif%%if(postPage.Games)% for %postPage.Games :{game |&lt;a href="%site.BaseURL%game/%game.Title;format="urlize"%"&gt;%game.Title;format="escapeforrssxml"%&lt;/a&gt;};separator=", "%%endif%%if(postPage.Platforms)% on %postPage.Platforms :{platform |&lt;a href="%site.BaseURL%platform/%platform.Abbreviation;format="urlize"%"&gt;%platform.Abbreviation;format="escapeforrssxml"%&lt;/a&gt;};separator=", "%%endif%&lt;/i&gt;&lt;/p&gt;
%postPage.RenderedContent;format="escapeforrssxml"%
%if(postPage.Ratings)%&lt;p&gt;&lt;i&gt;Rating: %postPage.Ratings :{rating |&lt;a href="%site.BaseURL%rating/%rating.Name;format="urlize"%"&gt;%rating.Name;format="escapeforrssxml"%&lt;/a&gt;};separator=", "%&lt;/i&gt;&lt;/p&gt;%endif%
      </description>
    </item>
}%
  </channel>
</rss>
>>
