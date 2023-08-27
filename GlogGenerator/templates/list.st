list(site,page) ::= <<
%partials/head(site,page)%

<body>
	<div class="container">
		<div class="left-col">
			<header id="header" class="inner">%partials/header(site,page)%</header>
		</div>
		<div class="mid-col">
			<div class="mid-col-container">
				<div class="list-header">
					<div class="title">%page.LinkedPostsCount% posts in <span class="%page.PageType%">%page.Title%</span></div>
				</div>
				<div id="content" class="inner">
%partials/archive(site,page)%
				</div>
			</div>
			<footer id="footer" class="inner">%partials/footer(site,page)%</footer>
		</div>
	</div>
</body>
</html>

>>
