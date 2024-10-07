games(site,page) ::= <<
%if(page.Games)%
<span class="games">%page.Games :{game |<a href="%site.BaseURL%game/%game.Title;format="urlize"%">%game.Title%</a>};separator=", "%</span>
%endif%
>>
