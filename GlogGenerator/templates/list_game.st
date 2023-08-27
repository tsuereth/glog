list_game(site,page) ::= <<
%partials/head(site,page)%

<body>
	<div class="container">
		<div class="left-col">
			<header id="header" class="inner">%partials/header(site,page)%</header>
		</div>
		<div class="mid-col">
			<div class="mid-col-container">

				<div class="gameinfo">
					
					<div class="title">%page.Title%</div>
					
%if(page.IgdbUrl)%
					
					<p class="link">Metadata from <a href="%page.IgdbUrl%">IGDB</a></p>
					
%endif%
					
%if(page.Tags)%
					
					<div class="tags">
%page.Tags :{tag |
						
						
						
						<a href="%site.BaseURL%tag/%tag;format="urlize"%">%tag%</a>};separator=",\n\n"%
						
						
					</div>
%endif%
					
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
