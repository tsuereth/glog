article_inhistory(site,page) ::= <<
    <div class="meta">
%if(!page.HideDate)%

	<span class="date">%post/date(page)%</span>
%endif%

	

%post/categories(site,page)%

	

%post/games(site,page)%

	

%post/platforms(site,page)%

    </div>
    <h1 class="title" itemprop="name"><a href="%page.Permalink%" itemprop="url">%page.Title%</a></h1>
    <div class="entry-content" itemprop="articleBody">
%page.RenderedContent%

%post/rating(site,page)%
    </div>
>>
