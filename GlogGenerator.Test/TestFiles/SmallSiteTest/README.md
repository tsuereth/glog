To refresh this test content, run:

```
./path/to/GlogGenerator build \
	--index-files-path GlogGenerator.Test/TestFiles/SmallSiteTest/sitedataindex \
	--cache-files-path GlogGenerator.Test/TestFiles/SmallSiteTest/igdbcache \
	-i GlogGenerator.Test/TestFiles/SmallSiteTest \
	-t GlogGenerator/templates \
	-u true \
	--igdb-client-id ... \
	--igdb-client-secret ... \
	-h "http://fakeorigin.com" \
	-o GlogGenerator.Test/TestFiles/SmallSiteBuild/public-expected
```
