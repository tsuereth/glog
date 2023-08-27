#!/usr/bin/env perl
use FindBin;
use lib $FindBin::Bin;
use strict;
use warnings;

use GameCache;
use Glog;

binmode STDOUT, ":utf8";

if(scalar(@ARGV) < 1) {
	die "Usage: ./check-tags.pl LOCAL_CACHE_FILE";
}

my $cache_file = $ARGV[0];

# Find all tags referenced in posts.
my @tags;
Glog::GetReferencedTags(\@tags);

# Create or load the local cache.
if(-f($cache_file)) {
	print "Opening local cache at \"$cache_file\" ...\n";
	GameCache::LoadFromFlatFile($cache_file);
}
else {
	print "Creating new cache to save at \"$cache_file\" ...\n";
	GameCache::Initialize();
}

# Check each referenced tag against the cache.
foreach my $tag(@tags) {
	if(!GameCache::ValidateTag($tag)) {
		print "No cache entry for tag \"$tag\"\n";
	}
}
