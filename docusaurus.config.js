// @ts-check

/** @type {import("@docusaurus/types").Config} */
const config = {
    title: "TF2 HUD Editor",
    tagline: "Install and customize your favorite custom Team Fortress 2 HUDs.",
    url: "https://cooolbros.github.io",
    baseUrl: "/TF2HUD.Editor/",
    favicon: "./docs/resources/logo.png",

    // GitHub pages deployment config.
    // If you aren"t using GitHub pages, you don"t need these.
    organizationName: "cooolbros", // Usually your GitHub org/user name.
    projectName: "TF2HUD.Editor", // Usually your repo name.
    deploymentBranch: "gh-pages",

    trailingSlash: true,

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
                blog: false,
                theme: {
                    customCss: require.resolve("./docs/resources/custom.css"),
                }
            })
        ]
    ],

    themes: [
        [
            require.resolve("@easyops-cn/docusaurus-search-local"),
            {
                docsRouteBasePath: "/",
            }
        ]
    ],

    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    themeConfig: {
        navbar: {
            style: "dark",
            title: "TF2 HUD Editor",
            logo: {
                alt: "My Site Logo",
                src: "https://www.editor.criticalflaw.ca/resources/logo.png",
            },
            items: [
                {
                    label: "Screenshots",
                    type: "doc",
                    docId: "screenshots",
                    position: "left",
                },
                {
                    label: "Troubleshooting",
                    type: "doc",
                    docId: "troubleshoot",
                    position: "left",
                },
                {
                    label: "How to Contribute",
                    type: "doc",
                    docId: "contribute",
                    position: "left",
                },
                {
                    label: "Credits",
                    type: "doc",
                    docId: "credits",
                    position: "left",
                },
                {
                    label: "Adding your HUD",
                    type: "doc",
                    docId: "json/intro",
                    position: "left",
                },
                {
                    label: "Blog",
                    href: "https://www.criticalflaw.ca/"
                },
                {
                    href: "https://github.com/CriticalFlaw/TF2HUD.Editor/",
                    label: "GitHub",
                    position: "right",
                },
            ],
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
                            href: "https://github.com/CriticalFlaw/TF2HUD.Editor",
                        },
                        {
                            label: "Discord",
                            href: "https://github.com/CriticalFlaw/TF2HUD.Editor",
                        },
                    ],
                }
            ],
            copyright: `Copyright © ${new Date().getFullYear()} My Project, Inc. Built with Docusaurus.`,
        }
    }
}

module.exports = config
