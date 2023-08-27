package GameCache;

use FindBin;
use lib $FindBin::Bin;
use strict;
use utf8;
use warnings;

use DBI;
use IGDB;

use open ":encoding(UTF-8)";

my $db = undef;
my $initialized = 0;

my @_tables = (
	{
		"category" => [
			"id INTEGER PRIMARY KEY",
			"name TEXT"
		]
	},
	{
		"collection" => [
			"id INTEGER PRIMARY KEY",
			"name TEXT"
		]
	},
	{
		"company" => [
			"id INTEGER PRIMARY KEY",
			"name TEXT"
		]
	},
	{
		"franchise" => [
			"id INTEGER PRIMARY KEY",
			"name TEXT"
		]
	},
	{
		"game" => [
			"id INTEGER PRIMARY KEY",
			"category INTEGER",
			"collection INTEGER",
			"franchise INTEGER",
			"franchises TEXT",
			"game_modes TEXT",
			"genres TEXT",
			"involved_companies TEXT",
			"name TEXT",
			"player_perspectives TEXT",
			"themes TEXT",
			"updated_at INTEGER",
			"url TEXT"
		]
	},
	{
		"game_mode" => [
			"id INTEGER PRIMARY KEY",
			"name TEXT"
		]
	},
	{
		"genre" => [
			"id INTEGER PRIMARY KEY",
			"name TEXT"
		]
	},
	{
		"involved_company" => [
			"id INTEGER PRIMARY KEY",
			"company INTEGER"
		]
	},
	{
		"player_perspective" => [
			"id INTEGER PRIMARY KEY",
			"name TEXT"
		]
	},
	{
		"theme" => [
			"id INTEGER PRIMARY KEY",
			"name TEXT"
		]
	}
);

sub Initialize {
	if($initialized) {
		die "Already initialized"
	}

	# Create or open the SQLite database.
	$db = DBI->connect(
		"dbi:SQLite:dbname=:memory:", "", "", {
			"RaiseError" => 1,
			"sqlite_unicode" => 1
		}
	);

	# Create cache tables, if they don't already exist.
	foreach my $tableref(@_tables) {
		my $table = (keys %{$tableref})[0];
		my $columns_string = "";
		foreach my $column(@{$tableref->{$table}}) {
			if($columns_string ne "") {
				$columns_string .= ", ";
			}
			$columns_string .= $column;
		}

		$db->do("CREATE TABLE IF NOT EXISTS $table (" . $columns_string . ");");
	}

	# Manually populate the category table.
	my %categories = IGDB::GetCategories();
	my $insertCategories = "INSERT OR REPLACE INTO category (id, name) VALUES ";
	my $insertedFirst = 0;
	foreach my $i(keys %categories) {
		if($insertedFirst == 1) {
			$insertCategories .= ", ";
		}

		my $category = $categories{$i};
		$insertCategories .= "($i, \"$category\")";

		$insertedFirst = 1;
	}
	$db->do($insertCategories);

	$initialized = 1;
	return 1;
}

sub LoadFromFlatFile {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to LoadFromFlatFile(path)";
	}
	elsif($initialized) {
		die "Already initialized"
	}

	my ($path) = @_;

	my $file;
	if(!open($file, $path)) {
		die "Failed to open flat database file \"$path\"";
	}

	# Create or open the SQLite database.
	$db = DBI->connect(
		"dbi:SQLite:dbname=:memory:", "", "", {
			"RaiseError" => 1,
			"sqlite_unicode" => 1
		}
	);

	# Execute the flat text file as database queries.
	while(my $line = <$file>) {
		my $st = $db->prepare($line);
		$st->execute();
	}

	close($file);

	# Validate the DB by checking for the existence of each table.
	my $st = $db->prepare("SELECT name FROM sqlite_master WHERE type=\"table\"");
	$st->execute();
	my %found;
	while(my $row = $st->fetchrow_arrayref) {
		$found{$row->[0]} = 1;
	}

	foreach my $tableref(@_tables) {
		my $table = (keys %{$tableref})[0];
		if(!defined($found{$table})) {
			warn "LoadFromFlatFile($path) cache is invalid: failed to find table $table";
			return 0;
		}
	}

	$initialized = 1;
	return 1;
}

sub SaveToFlatFile {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to SaveToFlatFile(path)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize first";
	}

	my ($path) = @_;

	my $file;
	if(!open($file, '>', $path)) {
		die "Failed to open output file \"$path\"";
	}

	# Prologue.
	print $file "PRAGMA foreign_keys=OFF;\n";
	print $file "BEGIN TRANSACTION;\n";

	foreach my $tableref(@_tables) {
		# Create a table.
		my $table = (keys %{$tableref})[0];
		my $columns_string = "";
		foreach my $column(@{$tableref->{$table}}) {
			if($columns_string ne "") {
				$columns_string .= ", ";
			}
			$columns_string .= $column;
		}

		print $file "CREATE TABLE $table (" . $columns_string . ");\n";

		# Populate that table.
		my $st = $db->prepare("SELECT * FROM $table");
		$st->execute();
		while(my $row = $st->fetchrow_arrayref) {
			my $values_str = "";

			for(my $i = 0; $i < scalar(@{$row}); $i++) {
				my $value = $row->[$i];

				if($tableref->{$table}->[$i] =~ m/ TEXT$/) {
					$value = $db->quote($value);
				}

				if($values_str ne "") {
					$values_str .= ",";
				}
				$values_str .= $value;
			}
			print $file "INSERT INTO $table VALUES($values_str);\n";
		}
	}

	# Epilogue.
	print $file "COMMIT;\n";

	close($file);
}

sub GetGameName {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to GetGameName(id)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($id) = @_;

	my $st = $db->prepare("SELECT name FROM game WHERE id = ?");
	$st->execute($id);
	if(my $row = $st->fetchrow_arrayref) {
		return $row->[0];
	}

	return 0;
}

sub GetGameLastUpdated {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to GetGameLastUpdated(id)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($id) = @_;

	my $st = $db->prepare("SELECT updated_at FROM game WHERE id = ?");
	$st->execute($id);
	if(my $row = $st->fetchrow_arrayref) {
		return $row->[0];
	}

	return 0;
}

sub GetGameTags {
	if(scalar(@_) < 2) {
		die "Insufficient arguments to GetGameTags(id, tags_listref)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($id, $tags_listref) = @_;

	my %tags_unique;

	my $st = $db->prepare("SELECT * FROM game WHERE id = ?");
	$st->execute($id);
	if(my $game = $st->fetchrow_hashref) {
		$st = $db->prepare("SELECT name FROM category WHERE id = ?");
		$st->execute($game->{"category"});
		if(my $row = $st->fetchrow_arrayref) {
			my $tag = $row->[0];
			if(($tag ne "") && !defined($tags_unique{lc($tag)})) {
				$tags_unique{lc($tag)} = 1;
				push(@{$tags_listref}, $tag);
			}
		}
		else {
			warn "Unknown game category " . $game->{"category"} . " for game ID $id";
		}

		if($game->{"collection"} != -1) {
			$st = $db->prepare("SELECT name FROM collection WHERE id = ?");
			$st->execute($game->{"collection"});
			my $row;
			if($row = $st->fetchrow_arrayref) {
				my $tag = $row->[0];
				if(($tag ne "") && !defined($tags_unique{lc($tag)})) {
					$tags_unique{lc($tag)} = 1;
					push(@{$tags_listref}, $tag);
				}
			}
		}

		if($game->{"franchise"} != -1) {
			$st = $db->prepare("SELECT name FROM franchise WHERE id = ?");
			$st->execute($game->{"franchise"});
			my $row;
			if($row = $st->fetchrow_arrayref) {
				my $tag = $row->[0];
				if(($tag ne "") && !defined($tags_unique{lc($tag)})) {
					$tags_unique{lc($tag)} = 1;
					push(@{$tags_listref}, $tag);
				}
			}
		}

		if($game->{"franchises"} ne "") {
			$st = $db->prepare("SELECT name FROM franchise WHERE id IN (" . $game->{"franchises"} . ")");
			$st->execute();
			while(my $row = $st->fetchrow_arrayref) {
				my $tag = $row->[0];
				if(($tag ne "") && !defined($tags_unique{lc($tag)})) {
					$tags_unique{lc($tag)} = 1;
					push(@{$tags_listref}, $tag);
				}
			}
		}

		if($game->{"involved_companies"} ne "") {
			my @companies;

			$st = $db->prepare("SELECT company FROM involved_company WHERE id IN (" . $game->{"involved_companies"} . ")");
			$st->execute();
			while(my $row = $st->fetchrow_arrayref) {
				push(@companies, $row->[0]);
			}

			if(scalar(@companies) > 0) {
				$st = $db->prepare("SELECT name FROM company WHERE id IN (" . join(",", @companies) . ")");
				$st->execute();
				while(my $row = $st->fetchrow_arrayref) {
					my $tag = $row->[0];
					if(($tag ne "") && !defined($tags_unique{lc($tag)})) {
						$tags_unique{lc($tag)} = 1;
						push(@{$tags_listref}, $tag);
					}
				}
			}
		}

		if($game->{"genres"} ne "") {
			$st = $db->prepare("SELECT name FROM genre WHERE id IN (" . $game->{"genres"} . ")");
			$st->execute();
			while(my $row = $st->fetchrow_arrayref) {
				my $tag = $row->[0];
				if(($tag ne "") && !defined($tags_unique{lc($tag)})) {
					$tags_unique{lc($tag)} = 1;
					push(@{$tags_listref}, $tag);
				}
			}
		}

		if($game->{"game_modes"} ne "") {
			$st = $db->prepare("SELECT name FROM game_mode WHERE id IN (" . $game->{"game_modes"} . ")");
			$st->execute();
			while(my $row = $st->fetchrow_arrayref) {
				my $tag = $row->[0];
				if(($tag ne "") && !defined($tags_unique{lc($tag)})) {
					$tags_unique{lc($tag)} = 1;
					push(@{$tags_listref}, $tag);
				}
			}
		}

		if($game->{"player_perspectives"} ne "") {
			$st = $db->prepare("SELECT name FROM player_perspective WHERE id IN (" . $game->{"player_perspectives"} . ")");
			$st->execute();
			while(my $row = $st->fetchrow_arrayref) {
				my $tag = $row->[0];
				if(($tag ne "") && !defined($tags_unique{lc($tag)})) {
					$tags_unique{lc($tag)} = 1;
					push(@{$tags_listref}, $tag);
				}
			}
		}

		if($game->{"themes"} ne "") {
			$st = $db->prepare("SELECT name FROM theme WHERE id IN (" . $game->{"themes"} . ")");
			$st->execute();
			while(my $row = $st->fetchrow_arrayref) {
				my $tag = $row->[0];
				if(($tag ne "") && !defined($tags_unique{lc($tag)})) {
					$tags_unique{lc($tag)} = 1;
					push(@{$tags_listref}, $tag);
				}
			}
		}
	}
}

sub UpdateGame {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to UpdateGame(game_hashref)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($game_hashref) = @_;

	my $id = $game_hashref->{"id"};

	if(!defined($game_hashref->{"category"})) {
		die "Missing category for game ID $id";
	}
	if(!defined($game_hashref->{"name"})) {
		die "Missing name for game ID $id";
	}

	my $st = $db->prepare("INSERT OR REPLACE INTO game (id, category, collection, franchise, franchises, game_modes, genres, involved_companies, name, player_perspectives, themes, updated_at, url) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
	$st->execute(
		$id,
		$game_hashref->{"category"},
		$game_hashref->{"collection"},
		$game_hashref->{"franchise"},
		$game_hashref->{"franchises"},
		$game_hashref->{"game_modes"},
		$game_hashref->{"genres"},
		$game_hashref->{"involved_companies"},
		$game_hashref->{"name"},
		$game_hashref->{"player_perspectives"},
		$game_hashref->{"themes"},
		$game_hashref->{"updated_at"},
		$game_hashref->{"url"}
	);
}

sub UpdateCollection {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to UpdateCollection(collection_hashref)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($collection_hashref) = @_;

	my $st = $db->prepare("INSERT OR REPLACE INTO collection (id, name) VALUES (?, ?)");
	$st->execute($collection_hashref->{"id"}, $collection_hashref->{"name"});
}

sub UpdateCompany {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to UpdateCompany(company_hashref)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($company_hashref) = @_;

	my $st = $db->prepare("INSERT OR REPLACE INTO company (id, name) VALUES (?, ?)");
	$st->execute($company_hashref->{"id"}, $company_hashref->{"name"});
}

sub UpdateFranchise {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to UpdateFranchise(franchise_hashref)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($franchise_hashref) = @_;

	my $st = $db->prepare("INSERT OR REPLACE INTO franchise (id, name) VALUES (?, ?)");
	$st->execute($franchise_hashref->{"id"}, $franchise_hashref->{"name"});
}

sub UpdateGameMode {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to UpdateGameMode(gamemode_hashref)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($gamemode_hashref) = @_;

	my $st = $db->prepare("INSERT OR REPLACE INTO game_mode (id, name) VALUES (?, ?)");
	$st->execute($gamemode_hashref->{"id"}, $gamemode_hashref->{"name"});
}

sub UpdateGenre {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to UpdateGenre(genre_hashref)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($genre_hashref) = @_;

	my $st = $db->prepare("INSERT OR REPLACE INTO genre (id, name) VALUES (?, ?)");
	$st->execute($genre_hashref->{"id"}, $genre_hashref->{"name"});
}

sub UpdateInvolvedCompany {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to UpdateInvolvedCompany(involved_company_hashref)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($involved_company_hashref) = @_;

	my $st = $db->prepare("INSERT OR REPLACE INTO involved_company (id, company) VALUES (?, ?)");
	$st->execute($involved_company_hashref->{"id"}, $involved_company_hashref->{"company"});
}

sub UpdatePlayerPerspective {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to UpdatePlayerPerspective(playerperspective_hashref)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($playerperspective_hashref) = @_;

	my $st = $db->prepare("INSERT OR REPLACE INTO player_perspective (id, name) VALUES (?, ?)");
	$st->execute($playerperspective_hashref->{"id"}, $playerperspective_hashref->{"name"});
}

sub UpdateTheme {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to UpdateTheme(theme_hashref)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($theme_hashref) = @_;

	my $st = $db->prepare("INSERT OR REPLACE INTO theme (id, name) VALUES (?, ?)");
	$st->execute($theme_hashref->{"id"}, $theme_hashref->{"name"});
}

sub ValidateTag {
	if(scalar(@_) < 1) {
		die "Insufficient arguments to ValidateTag(tag)";
	}
	elsif(!$initialized) {
		die "Must call GameCache::Initialize or GameCache::LoadFromFlatFile first";
	}

	my ($tag) = @_;

	my @tag_tables = ("category", "collection", "company", "franchise", "game_mode", "genre", "player_perspective", "theme");

	foreach my $tag_table(@tag_tables) {
		# Do a case-insensitive search.
		my $st = $db->prepare("SELECT COUNT(*) FROM $tag_table WHERE name = ? COLLATE NOCASE");
		$st->execute($tag);

		while(my $row = $st->fetchrow_arrayref) {
			if($row->[0] > 0) {
				return 1;
			}
		}
	}

	return 0;
}

1;
