package Glog;

use strict;
use utf8;
use warnings;

use File::Path;
use HTML::Entities;

use open ":encoding(UTF-8)";

sub _ReaddirRecursive {
	if(scalar(@_) < 4) {
		die "Insufficient arguments to _ReaddirRecursive(dir_path, dir_handle, addfile_callback, addfile_callbackparam)";
	}

	my ($dir_path, $dir_handle, $addfile_callback, $addfile_callbackparam) = @_;

	while(my $dirent = readdir($dir_handle)) {
		if(($dirent eq ".") || ($dirent eq "..") || ($dirent eq ".DS_Store")) {
			next;
		}
		my $dirent_path = $dir_path . '/' . $dirent;

		if(-d($dirent_path)) {
			opendir(my $subdir_handle, $dirent_path) || die "Failed to open directory \"$dirent_path\"";
			_ReaddirRecursive($dirent_path, $subdir_handle, $addfile_callback, $addfile_callbackparam);
			closedir($subdir_handle);
		}
		elsif(-f($dirent_path)) {
			&$addfile_callback($dirent_path, $addfile_callbackparam);
		}
	}
}

sub _GetGameNameFromMetadata {
	if(scalar(@_) < 2) {
		die "Insufficient arguments to _GetGameNameFromMetadata(path, gamenames_hashref)";
	}

	my ($path, $gamenames_hashref) = @_;

	open(my $file, $path) || die "Failed to open file \"$path\"";

	while(my $line = <$file>) {
		if($line =~ /^title:\s*"(.+?)"\s*$/i) {
			my $gamename = $1;
			my $urlized = UrlizeSegment($gamename);
			if(!defined($gamenames_hashref->{$urlized})) {
				$gamenames_hashref->{$urlized} = $gamename;
			}

			last;
		}
	}

	close($file);
}

sub _GetGameTagsFromMetadata {
	if(scalar(@_) < 2) {
		die "Insufficient arguments to _GetGameTagsFromMetadata(path, gametags_hashref)";
	}

	my ($path, $gametags_hashref) = @_;

	open(my $file, $path) || die "Failed to open file \"$path\"";

	while(my $line = <$file>) {
		if($line =~ /^tag:\s*\[(.+?)\]\s*$/i) {
			my @tags = split(",", $1);
			foreach my $tag(@tags) {
				# The tag string will be enclosed in double-quotes.
				$tag =~ s/^"//;
				$tag =~ s/"$//;
				$tag =~ s/\\"/"/g;

				my $urlized = UrlizeSegment($tag);
				if(!defined($gametags_hashref->{$urlized})) {
					$gametags_hashref->{$urlized} = $tag;
				}
			}

			last;
		}
	}

	close($file);
}

sub _GetGameNamesFromFile {
	if(scalar(@_) < 2) {
		die "Insufficient arguments to _GetGameNamesFromFile(path, gamenames_hashref)";
	}

	my ($path, $gamenames_hashref) = @_;

	open(my $file, $path) || die "Failed to open file \"$path\"";

	while(my $line = <$file>) {
		# Trigger on:

		# Items in `game = ["..."]` lists.
		while($line =~ s/^(game\s+=\s+\[)"(.+?)"[,\]]/$1/) {
			my $gamename = $2;
			my $urlized = UrlizeSegment($gamename);
			if(!defined($gamenames_hashref->{$urlized})) {
				$gamenames_hashref->{$urlized} = $gamename;
			}
		}

		# Text within `{{% game "..." %}}` shortcodes.
		while($line =~ s/\{\{%\s*game\s*"(.+?)"\s*%\}\}//) {
			my $gamename = $1;
			my $urlized = UrlizeSegment($gamename);
			if(!defined($gamenames_hashref->{$urlized})) {
				$gamenames_hashref->{$urlized} = $gamename;
			}
		}
	}

	close($file);
}

sub _GetTagsFromFile {
	if(scalar(@_) < 2) {
		die "Insufficient arguments to _GetTagsFromFile(path, tags_hashref)";
	}

	my ($path, $tags_hashref) = @_;

	open(my $file, $path) || die "Failed to open file \"$path\"";

	while(my $line = <$file>) {
		# Trigger on:

		# Text within `{{% tag "..." %}}` shortcodes.
		while($line =~ s/\{\{%\s*tag\s*"(.+?)"\s*%\}\}//) {
			my $tag = $1;
			my $urlized = UrlizeSegment($tag);
			if(!defined($tags_hashref->{$urlized})) {
				$tags_hashref->{$urlized} = $tag;
			}
		}
	}

	close($file);
}

sub GetAllGameNames {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to GetAllGameNames(games_listref)";
	}

	my ($games_listref) = @_;

	# This hash will key on urlized game names, to enforce uniqueness.
	my %gamenames;

	# Look for titles of existing games.
	my $games_path = "content/game";
	opendir(my $games_handle, $games_path) || die "Failed to open games directory \"$games_path\"";
	_ReaddirRecursive($games_path, $games_handle, \&_GetGameNameFromMetadata, \%gamenames);
	closedir($games_handle);

	# Then look for game names referenced by posts.
	my $posts_path = "content/post";
	opendir(my $posts_handle, $posts_path) || die "Failed to open posts directory \"$posts_path\"";
	_ReaddirRecursive($posts_path, $posts_handle, \&_GetGameNamesFromFile, \%gamenames);
	closedir($posts_handle);

	# Then look for game names referenced by other pages.
	my @scan_pages = (
		"content/backlog.md",
		"content/upcoming.md"
	);

	foreach my $path(@scan_pages) {
		_GetGameNamesFromFile($path, \%gamenames);
	}

	push(@{$games_listref}, values %gamenames);
}

sub GetValidTags {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to GetValidTags(tags_listref)";
	}

	my ($tags_listref) = @_;

	# This hash will key on urlized tags, to enforce uniqueness.
	my %tags;

	# Look for tags in game metadata.
	my $games_path = "content/game";
	opendir(my $games_handle, $games_path) || die "Failed to open games directory \"$games_path\"";
	_ReaddirRecursive($games_path, $games_handle, \&_GetGameTagsFromMetadata, \%tags);
	closedir($games_handle);

	push(@{$tags_listref}, values %tags);
}

sub GetReferencedTags {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to GetReferencedTags(tags_listref)";
	}

	my ($tags_listref) = @_;

	# This hash will key on urlized tags, to enforce uniqueness.
	my %tags;

	# Look for tags referenced by posts.
	my $posts_path = "content/post";
	opendir(my $posts_handle, $posts_path) || die "Failed to open posts directory \"$posts_path\"";
	_ReaddirRecursive($posts_path, $posts_handle, \&_GetTagsFromFile, \%tags);
	closedir($posts_handle);

	push(@{$tags_listref}, values %tags);
}

sub NormalizeGameName {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to NormalizeGameName(title)";
	}

	my ($title) = @_;

	# HTML-unescape.
	$title = decode_entities($title);

	# Replace smartquotes.
	$title =~ s/[\N{U+2018}\N{U+2019}]/'/g;
	$title =~ s/[\N{U+201C}\N{U+201D}]/"/g;

	# Remove a trailing release-year annotation.
	$title =~ s/\s*\(\d\d\d\d\)$//;

	return $title;
}

sub GetIgdbId {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to GetIgdbId(title)";
	}

	my ($title) = @_;

	my $urlized = UrlizeSegment($title);
	my $game_file = "content/game/" . $urlized . "/_index.md";

	my $game_md = undef;
	if(!open($game_md, $game_file)) {
		warn "Failed to open game file \"$game_file\"";
		return undef;
	}

	my $igdb_id = undef;

	while(my $line = <$game_md>) {
		if($line =~ /^\s*igdb_id\s*:\s*(\d+)\s*$/) {
			$igdb_id = $1;
			last;
		}
	}

	close($game_md);

	return $igdb_id;
}

# Try to match Hugo's `urlizeSegment` method.
sub UrlizeSegment {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to UrlizeSegment(string)";
	}

	my ($string) = @_;

	# 1. Trim leading and trailing spaces.
	$string =~ s/^\s+//;
	$string =~ s/\s+$//;

	# 2. Replace spaces with hyphens.
	$string =~ s/ /-/g;

	# 3. "Unicode Sanitize" -- for simplicity, just whitelist certain characters.
	$string =~ s/[^A-Za-z0-9\.\/\\_\-\#\+\~]//g;

	# 4. Collapse multiple hyphens into one.
	$string =~ s/-+/-/g;

	# 5. Lowercase.
	$string = lc($string);

	return $string;
}

sub GetGameMetadata {
	if(scalar(@_) < 2) {
		die "Insufficient arguments to GetGameMetadata(title, metadata_hashref)";
	}

	my ($title, $metadata_hashref) = @_;

	my $urlized = UrlizeSegment($title);

	my $game_dir = "content/game/" . $urlized;
	my $game_file = $game_dir . "/_index.md";

	if(!(-d($game_dir)) || !(-f($game_file))) {
		return;
	}

	open(my $game_md, $game_file) || die "Failed to open game file \"$game_file\"";
	my $collecting = 0;
	while(my $line = <$game_md>) {
		# Collect keys and values between "---" sentinel lines.
		if(($collecting == 0) && ($line =~ /^\-\-\-$/)) {
			$collecting = 1;
		}
		elsif($collecting == 1) {
			if($line =~ /^\-\-\-$/) {
				last;
			}

			if($line =~ /^(\S+)\s*\:\s*(.*?\S)\s*$/) {
				my($key, $value) = ($1, $2);
				if($value =~ /^\[(.*)\]$/) {
					# Decode the array.
					my $list_str = $1;
					$value = [];
					while($list_str =~ s/^\"([^"]+)\"(, |\z)//) {
						push(@{$value}, $1);
					}
				}
				elsif($value eq "true") {
					$value = 1;
				}
				elsif($value eq "false") {
					$value = 0;
				}

				$metadata_hashref->{$key} = $value;
			}
		}
	}
	close($game_md);
}

sub WriteGameMetadata {
	if(scalar(@_) < 4) {
		die "Insufficient arguments to WriteGameMetadata(title, igdb_id, igdb_url, tags_listref)";
	}

	my ($title, $igdb_id, $igdb_url, $tags_listref) = @_;

	my $urlized = UrlizeSegment($title);

	my $game_dir = "content/game/" . $urlized;
	my $game_file = $game_dir . "/_index.md";

	if(!(-d($game_dir))) {
		File::Path::make_path($game_dir);
	}

	# Don't write smartquotes.
	# Smartquotes are dumb.
	$title =~ s/[\N{U+2018}\N{U+2019}]/'/g;
	$title =~ s/[\N{U+201C}\N{U+201D}]/"/g;

	# Decode any HTML entities first, to avoid double-encoding.
	$title = decode_entities($title);

	# Now make the title HTML-safe.
	$title = encode_entities($title, '<>&');
	$title = encode_entities($title, '^\x00-\x7f');

	my @tags_escaped = @{$tags_listref};

	# Make tag strings JSON-safe.
	for (my $i = 0; $i < scalar(@tags_escaped); $i++) {
		$tags_escaped[$i] =~ s/"/\\"/g;
	}

	my $tags_str = "\"" . join("\", \"", @tags_escaped) . "\"";

	open(my $game_md, '>', $game_file) || die "Failed to open game file \"$game_file\"";
	print $game_md <<ENDMD;
---
title: "$title"
igdb_id: $igdb_id
igdb_url: "$igdb_url"
tag: [$tags_str]
---
ENDMD
	close($game_md);
}

sub _GetPostMetadataFromFile {
	if(scalar(@_) < 2) {
		die "Insufficient arguments to _GetPostMetadataFromFile(path, metadata_listref)";
	}

	my ($path, $metadata_listref) = @_;

	open(my $file, $path) || die "Failed to open file \"$path\"";

	my %metadata;
	my $autocollect = 0;
	while(my $line = <$file>) {
		# Auto-collect keys and values between "+++" sentinel lines.
		if(($autocollect == 0) && ($line =~ /^\+\+\+$/)) {
			$autocollect = 1;
		}
		elsif($autocollect == 1) {
			if($line =~ /^\+\+\+$/) {
				$autocollect = 2; # Prevent autocollect from being turned back on later.
				next;
			}

			if($line =~ /^(\S+)\s*=\s*(.*?\S)\s*$/) {
				my($key, $value) = ($1, $2);
				if($value =~ /^\["(.+?)"\]$/) {
					my @listval = split(/"\s*,\s*"/, $1);
					$value = \@listval;
				}
				elsif($value eq "[]") {
					$value = undef;
				}
				elsif($value =~ /^"(.+?)"$/) {
					$value = $1;
				}
				elsif($value eq "true") {
					$value = 1;
				}
				elsif($value eq "false") {
					$value = 0;
				}

				$metadata{$key} = $value;
			}
		}
		else {
			# Capture some non-standard post patterns.
			if($line =~ /^<b>Better than<\/b>: /i) {
				my @gamelist;
				if(defined($metadata{"betterthan_games"})) {
					@gamelist = @{$metadata{"betterthan_games"}};
				}

				while($line =~ s/\{\{%\s*game\s*"(.+?)"\s*%\}\}//) {
					push(@gamelist, $1);
				}

				if(scalar(@gamelist) > 0) {
					$metadata{"betterthan_games"} = \@gamelist;
				}
			}
			elsif($line =~ /^<b>Not as good as<\/b>: /i) {
				my @gamelist;
				if(defined($metadata{"notasgoodas_games"})) {
					@gamelist = @{$metadata{"notasgoodas_games"}};
				}

				while($line =~ s/\{\{%\s*game\s*"(.+?)"\s*%\}\}//) {
					push(@gamelist, $1);
				}

				if(scalar(@gamelist) > 0) {
					$metadata{"notasgoodas_games"} = \@gamelist;
				}
			}
		}
	}

	close($file);

	# Don't save metadata from draft posts.
	if(defined($metadata{"draft"}) && $metadata{"draft"}) {
		return;
	}

	push(@{$metadata_listref}, \%metadata);
}

sub GetAllPostMetadata {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to GetAllPostMedata(metadata_listref)";
	}

	my ($metadata_listref) = @_;

	my $posts_path = "content/post";
	opendir(my $posts_handle, $posts_path) || die "Failed to open posts directory \"$posts_path\"";
	_ReaddirRecursive($posts_path, $posts_handle, \&_GetPostMetadataFromFile, $metadata_listref);
	closedir($posts_handle);
}

1;
