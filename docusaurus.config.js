// @ts-check

/** @type {import("@docusaurus/types").Config} */
const config = {
    title: "TF2 HUD Editor",
    tagline: "Install and customize your favorite custom Team Fortress 2 HUDs.",
    url: "https://www.editor.criticalflaw.ca/",
    baseUrl: "/",
    favicon: "https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/docs/resources/logo.png",

    // GitHub pages deployment config.
    // If you aren"t using GitHub pages, you don"t need these.
    organizationName: "CriticalFlaw", // Usually your GitHub org/user name.
    projectName: "TF2HUD.Editor", // Usually your repo name.
    deploymentBranch: "gh-pages",

    // Even if you don"t use internalization, you can use this field to set useful
    // metadata like html lang. For example, if your site is Chinese, you may want
    // to replace "en" with "zh-Hans".
    i18n: {
        defaultLocale: "en",
        locales: ["en"],
    },

    presets: [
        [
            "classic",
            /** @type {import("@docusaurus/preset-classic").Options} */
            ({
                docs: {
                    routeBasePath: "/",
                    sidebarPath: require.resolve("./docs/sidebars.js"),

                    // Please change this to your repo.
                    // Remove this to remove the "edit this page" links.
                    editUrl: "https://github.com/CriticalFlaw/TF2HUD.Editor/tree/master",

                    showLastUpdateTime: true
                },
                pages: false,
                blog: false,
                theme: {
                    customCss: require.resolve("./docs/resources/custom.css"),
                },
                googleAnalytics: {
                    trackingID: "UA-141789564-1",
                    anonymizeIP: true
                }
            })
        ]
    ],

    themes: [
        [
            require.resolve("@easyops-cn/docusaurus-search-local"),
            {
                docsRouteBasePath: "/"
            }
        ]
    ],

    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    themeConfig: {
        navbar: {
            style: "dark",
            title: "TF2 HUD Editor",
            logo: {
                alt: "TF2HUD.Editor Logo",
                src: "https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/docs/resources/logo.png",
            },
            items: [
                {
                    label: "Screenshots",
                    type: "doc",
                    docId: "screenshots",
                    position: "left"
                },
                {
                    label: "Troubleshooting",
                    type: "doc",
                    docId: "troubleshoot",
                    position: "left"
                },
                {
                    label: "How to Contribute",
                    type: "doc",
                    docId: "contribute",
                    position: "left"
                },
                {
                    label: "Credits",
                    type: "doc",
                    docId: "credits",
                    position: "left"
                },
                {
                    label: "Adding your HUD",
                    type: "doc",
                    docId: "json/intro",
                    position: "left"
                },
                {
                    label: "Blog",
                    href: "https://www.criticalflaw.ca/"
                },
                {
                    href: "https://github.com/CriticalFlaw/TF2HUD.Editor/",
                    position: "right",
                    className: "header-github-link"
                }
            ]
        },
        footer: {
            links: [
                {
                    title: "Community",
                    items: [
                        {
                            label: "Github",
                            href: "https://github.com/CriticalFlaw/TF2HUD.Editor",
                        },
                        {
                            label: "Twitter",
                            href: "https://twitter.com/CriticalFlaw_",
                        },
                        {
                            label: "Discord",
                            href: "https://discord.gg/hTdtK9vBhE",
                        }
                    ]
                }
            ],
            copyright: `Copyright © ${new Date().getFullYear()} My Project, Inc. Built with Docusaurus.`
        }
    }
}

module.exports = config
