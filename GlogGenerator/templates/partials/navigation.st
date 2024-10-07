navigation(site,page) ::= <<
<ul class="main">
	<li><span class="nav-title">Playing:</span>
		<ul>
%site.NowPlaying :{title |
			<li class="nav-item"><a href="%site.BaseURL%game/%title;format="urlize"%">%title%</a></li>
}%
		</ul>
	</li>
	<li><span class="nav-title">Glogging:</span>
		<ul>
			<li class="nav-item"><a href="%site.BaseURL%">Newest Posts</a></li>
			<li class="nav-item"><a href="%site.BaseURL%upcoming/">Upcoming Games</a></li>
			<li class="nav-item"><a href="%site.BaseURL%backlog/">Insufferable Backlog</a></li>
		</ul>
	</li>
	<li><span class="nav-title">Categories:</span>
		<ul>
%site.CategoriesSorted :{category |
			<li class="nav-item"><a href="%site.BaseURL%category/%category;format="urlize"%/">%category%</a></li>
}%
		</ul>
	</li>
	<li><span class="nav-title">Archives:</span>
		<ul>
			<li class="nav-item"><a href="%site.BaseURL%game/">All Games</a></li>
			<li class="nav-item"><a href="%site.BaseURL%platform/">All Platforms</a></li>
			<li class="nav-item"><a href="%site.BaseURL%tag/">All Tags</a></li>
			<li class="nav-item"><a href="%site.BaseURL%post/">All Posts</a></li>
		</ul>
	</li>
</ul>
>>
