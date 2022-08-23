#I __SOURCE_DIRECTORY__

open System
open System.Diagnostics
open System.IO

module Log =
    let private log kind fmt =
        Printf.kprintf (fun s ->
            let now = DateTime.Now
            let msg = sprintf "[%s] [%s] %s" (now.ToString("s")) kind s

            printfn "%s" msg) fmt

    let fail fmt = log "Fail" fmt

    let info fmt = log "Info" fmt

module Command =
    let runCommand (exe : string) (args : string) =
        Log.info "  %s %s" exe args
        let p = Process.Start(exe, args)
        p.WaitForExit()

    let runDotnet (assemblyPath : string) (tool : string) (args : string) =
        let args' = sprintf "%s \"%s\" %s" tool assemblyPath args
        runCommand "dotnet" args'

let assemblyName = "Validus.Tests"
let assemblyPath = Path.Join(__SOURCE_DIRECTORY__, "test", assemblyName)

if not(Directory.Exists(assemblyPath)) then
    Log.fail "Invalid assembly: %s" assemblyPath
    failwith "Invalid assembly"

Command.runDotnet assemblyPath "clean" "-c Debug --nologo --verbosity quiet"
Command.runDotnet assemblyPath "restore" "--force --force-evaluate --nologo --verbosity quiet"
Command.runDotnet assemblyPath "watch --project" "-- test"