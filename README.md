# TopBestStories
Requires Visual Studio 2022 Community Edition 

solution called test.sln
2 project sub-folders

/test - (named originally for the Santander 'test') - but better renamed to 'TopHackerNewStories' (as is the actual Project name)

/TopHackerNewStoriesTest

build and run/debug the TopHackerNewStories in VS2022
either use test.http  in  TopHackerNewStories   to make requests  ( scratch-pad to make http calls)

GET {{test_HostAddress}}/TopHackerNewsStories?numStories=15

or use the generated SwaggerDocs that will launch automatically - 
eg. https://localhost:7245/swagger/index.html
on the 'Try it out' button


Run unit tests (Nunit) project  TopHackerNewStories.Tests from VS2022 to test
the individual components

Assumptions - HackerNews beststories.json call always returns the best stories in descending score order (which is not explicitly stated  on Api documentation as far I can see) but
is seen in practice  [and  if otherwise make an inefficient Api as score is not returned so can only be ordered on getting all the details (200 count )) to sort]

Performance - Avoiding over load of HackerNews api
----------------------------------------------------
Implements a caching component for story details - cache stategy at the moment -  cached the story details by Id - and only removing from cache once a 
threshold of cached items count is met -  being bigger than current best stories list by 1/2 - so   1/3 of cache is definitely for stories no longer in best stories list
- just remove those stale items - requires miminal processing overhead to keep the cache size bounded - only check for potential cache purge with simple count comparison per request
  
- Future  = Can change cache logic to remove  (still valid) storyIds after certain epxiry time so that  these  can get re-fetched to show updates to the score

