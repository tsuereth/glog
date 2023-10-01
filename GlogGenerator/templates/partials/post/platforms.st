platforms(site,page) ::= <<

%if(page.Platforms)%

<span class="platforms">
%page.Platforms :{platform |
	

	
	
		<a href="%site.BaseURL%platform/%platform.Abbreviation;format="urlize"%">%platform.Abbreviation%</a>};separator=",\n\n"%


</span>
%endif%

>>
