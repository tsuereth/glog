single(site,page) ::= <<
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
						<article class="post" itemscope itemtype="http://schema.org/BlogPosting">
							%partials/article(site,page)%
						</article>
				    </div>
				</div>
			</div>
			<footer id="footer" class="inner">%partials/footer(site,page)%</footer>
		</div>
	</div>
</body>
</html>
>>
