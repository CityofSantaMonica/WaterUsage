# Water Usage

Water usage information from the City of Santa Monica.

The data produced by this service is open and published at:  
https://data.smgov.net/Public-Services/Water-Usage/4nnq-5vzx

This service is an implementation of our [`SODA.NET`](https://github.com/CityofSantaMonica/SODA.NET) library for publishing
data to a [Socrata Open Data](https://dev.socrata.com) portal

License: [MIT](LICENSE.txt)

## Development Notes

Developed using [Visual Studio Community Edition](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx).
Use nuget to obtain dependencies.

### Configuration files

We omit certain configuration files from source control. The solution will not
compile without these files. Our [`WaterUsage.config`](https://github.com/CityofSantaMonica/WaterUsage.config) repo
provides an example of each required file, with the required settings and sample
values. The sample values should be replaced with your settings in order to run
the solution locally.

#### One time, by hand

Copy and paste each file, as-is, into the corresponding directory in your local
copy of this repository.

#### Automagic, with tracking

Add a remote pointing to the sample config repository (we'll call it *config*)

```bash
$ git remote add config https://github.com/CityofSantaMonica/WaterUsage.config
```

Fetch the latest history (with output)

```bash
$ git fetch config
warning: no common commits
remote: Counting objects: N, done.
remote: Compressing objects: 100% (N/N), done.
remote: Total N (delta X), reused Y (delta Z)
Unpacking objects: 100% (N/N), done.
From github.com:CityofSantaMonica/waterusage.config
 * [new branch]      master -> config/master
```

Finally, a bit of `git` magic: pack the necessary folders/files into an archive
then immediately unpack them into the repo root

```bash
$ git archive --format=tar config/master | tar xf -
```
