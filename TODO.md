# Todo

## General user interface
- Host should control -- and give to its controls if necessary -- the solution path (or solution directory)
- Semantic version updater should be independent from the rest of the UI, and not be bound on specific versions (though it can be initialized at a particular version)

## Assembly version checking
- UI: Assembly (DLL) version graph
  - "Open directory" should be initialized on the solution folder, given by the host
- UI: Solution NuGet package analysis
  - Find a better way to display the data?
- UI: Project version analysis
  - Detach the analysis from the semantic version updater

## Github project analysis
- Create a tool that can take a Github repository (ie. Invenietis/ck-core) and perform analysis on select files
  - The tool should ideally grab the tree of a commit/branch/ref, examine it, and download select files for analysis. The Github JSON API offers all this, and some implementation can be offered from https://github.com/erikzaadi/GithubSharp .