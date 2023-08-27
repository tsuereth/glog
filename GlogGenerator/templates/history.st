history(site,page) ::= <<
%partials/head(site,page)%

<body>
	<div class="container">
		<div class="left-col">
			<header id="header" class="inner">%partials/header(site,page)%</header>
		</div>
		<div class="mid-col">
			<div class="mid-col-container">
				<div id="content" class="inner">
				    
					<div itemscope itemtype="http://schema.org/Blog">
					
%page.HistoryPosts :{postPage |
					    <article class="post" itemprop="blogPost" itemscope itemtype="http://schema.org/BlogPosting">
%partials/article_inhistory(site,postPage)%
</article>
};separator="\t\n\t\n"%
                    
				    </div>
				    <nav id="pagenavi">
				        
					    %if(!page.HidePrevLink)%
					        <a href="%site.BaseURL%%page.PrevLinkRelative%" class="prev">Prev</a>
					        
					    %endif%
					    
					    %if(!page.HideNextLink)%
					        <a href="%site.BaseURL%%page.NextLinkRelative%" class="next">Next</a>
					        
					    %endif%
					    <div class="center"><a href="%site.BaseURL%post/">Blog Archives</a></div>
					</nav>
				</div>
			</div>
			<footer id="footer" class="inner">%partials/footer(site,page)%</footer>
		</div>
	</div>
</body>
</html>

>>
