categories(site,page) ::= <<
%if(page.Categories)%
<span class="categories">%page.Categories :{category |<a href="%site.BaseURL%category/%category.Name;format="urlize"%">%category.Name%</a>};separator=", "%</span>
%endif%
>>
