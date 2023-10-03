article(site,page) ::= <<
<div class="meta">
%if(!page.HideDate)%

	<span class="date">%post/date(page)%</span>
%endif%

	

%post/categories(site,page)%

	

%post/games(site,page)%

	

%post/platforms(site,page)%

</div>
<h1 class="title" itemprop="name"><a href="%page.Permalink%" itemprop="url">%page.Title%</a></h1>
<div class="entry-content" id="page_%page.HashCode%" itemprop="articleBody">
%page.RenderedContent%

%post/rating(site,page)%
</div>
<script type="text/javascript">
var body_width = (document.body.innerWidth || document.body.clientWidth) * 0.8;
var body_height = (document.body.innerHeight || document.body.clientHeight) * 0.8;
var body_sizeratio = body_width / body_height;

var page_el = document.getElementById("page_%page.HashCode%");
var page_images = page_el.querySelectorAll("img");
for (var page_image of page_images) {
	var img_width = page_image.getAttribute("width");
	var img_height = page_image.getAttribute("height");
	var img_sizeratio = img_width / img_height;
	if (img_sizeratio > body_sizeratio) {
		img_width = body_width;
		img_height = img_width / img_sizeratio;
	}
	else {
		img_height = body_height;
		img_width = img_height * img_sizeratio;
	}

	var img_anchor = document.createElement("a");
	img_anchor.setAttribute("href", page_image.getAttribute("src"));
	img_anchor.classList.add("imgwrap");
	img_anchor.setAttribute("data-pswp-width", img_width);
	img_anchor.setAttribute("data-pswp-height", img_height);
	page_image.parentNode.insertBefore(img_anchor, page_image);
	img_anchor.appendChild(page_image);
}

var page_%page.HashCode%_lightbox = new PhotoSwipeLightbox({
	gallery: "#page_%page.HashCode%",
	children: "a.imgwrap",
	pswpModule: PhotoSwipe
});
page_%page.HashCode%_lightbox.init();
</script>

>>
