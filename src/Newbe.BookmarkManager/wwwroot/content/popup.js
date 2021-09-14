(async () => {
    const folderTitle = "Amazing Favorites"
    const managerTabTitle = "Amazing Favorites";
    const tabs = await browser.tabs.query({
        active: true,
        currentWindow: true
    });
    if (tabs.length > 0 && tabs[0].url && tabs[0].title) {
        const tab = tabs[0];
        const bks = await browser.bookmarks.search({
            url: tab.url
        })
        let bkNode = null;
        if (bks.length) {
            bkNode = bks[0];
            console.log("bookmark already exists");
        } else {
            // create amazing favorites bookmark folder
            const nodes = await browser.bookmarks.search({
                title: folderTitle
            })
            let folderNode = null;
            let createFolder = false;
            if (nodes.length) {
                folderNode = nodes[0]
                if (folderNode.type !== "folder") {
                    createFolder = true
                }
            } else {
                createFolder = true
            }
            if (createFolder) {
                folderNode = await browser.bookmarks.create({
                    title: folderTitle
                })
            }

            // create bookmark
            bkNode = await browser.bookmarks.create({
                title: tab.title,
                url: tab.url,
                parentId: folderNode.id
            })
            console.log("new bookmark created");
        }
        const managerTabs = await browser.tabs.query({
            title: managerTabTitle
        });
        if (managerTabs.length > 0) {
            await browser.tabs.update(managerTabs[0].id, {
                active: true
            });

            await browser.storage.local.set({
                "afEvent": {
                    message: {
                        typeCode: "TriggerEditBookmarkEvent",
                        payloadJson: JSON.stringify({
                            title: bkNode.title,
                            url: bkNode.url,
                            tabId: tab.id
                        })
                    },
                    utcNow: Math.floor(Date.now() / 1000)
                }
            });
        } else {
            await browser.tabs.create({
                url: "/Manager/index.html"
            });

            await browser.storage.local.set({
                "afEvent": {
                    message: {
                        typeCode: 'UserClickAfIconEvent',
                        payloadJson: JSON.stringify({
                            tabId: tab.id
                        })
                    },
                    utcNow: Math.floor(Date.now() / 1000)
                }
            });
        }
    }
})();