archive(site,page) ::= <<
<div itemscope itemtype="http://schema.org/Blog">
%page.LinkedPosts :{linkedPost |
	<section class="archives">
		<article itemprop="blogPost" itemscope itemtype="http://schema.org/BlogPosting">
			<div class="meta">
				<span class="date"><time datetime='%linkedPost.Date;format="justdate"%' data-updated="true" itemprop="datePublished">%linkedPost.Date;format="justdate"%</time></span>
%if(linkedPost.Categories)%
				<span class="categories">%linkedPost.Categories :{category |<a href="%site.BaseURL%category/%category;format="urlize"%">%category%</a>};separator=", "%</span>
%endif%
%if(linkedPost.Games)%
				<span class="games">%linkedPost.Games :{game |<a href="%site.BaseURL%game/%game;format="urlize"%">%game%</a>};separator=", "%</span>
%endif%
%if(linkedPost.Platforms)%
				<span class="platforms">%linkedPost.Platforms :{platform |<a href="%site.BaseURL%platform/%platform;format="urlize"%">%platform%</a>};separator=", "%</span>
%endif%
			</div>
			<h1 class="title" itemprop="name"><a href="%site.BaseURL%%linkedPost.PermalinkRelative%">%linkedPost.Title%</a></h1>
		</article>
	</section>
}%
</div>
>>
