import pkg from "octokit";
import { exit } from "process";
import fs from "fs";

const { Octokit, App, Action } = pkg;

const octokit = new Octokit({ auth: process.env.GITHUB_TOKEN });

async function dispatch_workflow(repo, workflow, ref) {
//   let result = await octokit.request("GET /repos/{owner}/{repo}/actions/workflows/main.yml", {
//     owner: "Off-Live",
//     repo: repo,
//   });

//   console.log(result);

  const versionTag = process.env.VERSION_TAG;
  console.log(versionTag);

  result = await octokit.request("POST /repos/{owner}/{repo}/actions/workflows/{workflow_id}/dispatches", {
    owner: "Off-Live",
    repo: repo,
    workflow_id: workflow,
    ref: ref,
    inputs: {
      tag: versionTag,
    },
  });

  console.log(result);
}

await dispatch_workflow("myty-creator-viewer", "build.yml", "main");


exit(0);