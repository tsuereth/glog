rating(site,page) ::= <<
%if(page.Ratings)%
<div class="rating">Rating: %page.Ratings :{rating |<a href="%site.BaseURL%rating/%rating.Name;format="urlize"%">%rating.Name%</a>};separator=", "%</div>
%endif%
>>
