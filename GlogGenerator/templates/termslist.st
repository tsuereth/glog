termslist(site,page) ::= <<
%partials/head(site,page)%

<body>
	<div class="container">
		<div class="left-col">
			<header id="header" class="inner">%partials/header(site,page)%</header>
		</div>
		<div class="mid-col">
			<div class="mid-col-container">
				<div class="list-header">
					<div class="title">%page.TermsCount% %page.PageType%</div>
				</div>
				<div id="content" class="inner">
					<div itemscope itemtype="http://schema.org/Blog">
%page.Terms :{term |
                        
						<div class="list-entry %page.PageType%" itemprop="name"><a href="%site.BaseURL%%page.TermsType%/%term;format="urlize"%/">%term%</a></div>
}%
					</div>
				</div>
			</div>
			<footer id="footer" class="inner">%partials/footer(site,page)%</footer>
		</div>
	</div>
</body>
</html>

>>
