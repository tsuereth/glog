platforms(site,page) ::= <<

%if(page.Platforms)%

<span class="platforms">
%page.Platforms :{platform |
	

	
	
		<a href="%site.BaseURL%platform/%platform.Name;format="urlize"%">%platform.Name%</a>};separator=",\n\n"%


</span>
%endif%

>>
