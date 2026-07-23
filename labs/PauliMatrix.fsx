module PauliMatrix

open System
open System.IO
open System.Numerics
open System.Text.Json
open Encodings

let private matrix rows = array2D rows

let private singlePauli symbol =
    match symbol with
    | 'I' ->
        matrix [
            [ Complex.One; Complex.Zero ]
            [ Complex.Zero; Complex.One ]
        ]
    | 'X' ->
        matrix [
            [ Complex.Zero; Complex.One ]
            [ Complex.One; Complex.Zero ]
        ]
    | 'Y' ->
        matrix [
            [ Complex.Zero; -Complex.ImaginaryOne ]
            [ Complex.ImaginaryOne; Complex.Zero ]
        ]
    | 'Z' ->
        matrix [
            [ Complex.One; Complex.Zero ]
            [ Complex.Zero; -Complex.One ]
        ]
    | other -> invalidArg "symbol" (sprintf "Unknown Pauli symbol %c" other)

let private kronecker (left: Complex[,]) (right: Complex[,]) =
    let leftRows = Array2D.length1 left
    let leftCols = Array2D.length2 left
    let rightRows = Array2D.length1 right
    let rightCols = Array2D.length2 right

    Array2D.init
        (leftRows * rightRows)
        (leftCols * rightCols)
        (fun row col ->
            left[row / rightRows, col / rightCols]
            * right[row % rightRows, col % rightCols])

let private signatureMatrix (signature: string) =
    signature
    |> Seq.rev
    |> Seq.map singlePauli
    |> Seq.reduce kronecker

let private multiply (left: Complex[,]) (right: Complex[,]) =
    let rows = Array2D.length1 left
    let inner = Array2D.length2 left
    let cols = Array2D.length2 right

    if inner <> Array2D.length1 right then
        invalidArg "right" "Matrix dimensions do not align"

    Array2D.init rows cols (fun row col ->
        let mutable value = Complex.Zero
        for index in 0 .. inner - 1 do
            value <- value + left[row, index] * right[index, col]
        value)

let private identity dimension =
    Array2D.init dimension dimension (fun row col ->
        if row = col then Complex.One else Complex.Zero)

let private trace (value: Complex[,]) =
    [| for index in 0 .. Array2D.length1 value - 1 -> value[index, index] |]
    |> Array.fold (+) Complex.Zero

let hamiltonianMatrix (hamiltonian: PauliRegisterSequence) =
    let terms =
        hamiltonian.DistributeCoefficient.SummandTerms
        |> Array.filter (fun term -> Complex.Abs term.Coefficient > 1e-12)

    if terms.Length = 0 then
        invalidArg "hamiltonian" "Hamiltonian has no nonzero terms"

    let dimension = pown 2 terms[0].Signature.Length
    let result = Array2D.zeroCreate<Complex> dimension dimension

    for term in terms do
        let pauli = signatureMatrix term.Signature
        for row in 0 .. dimension - 1 do
            for col in 0 .. dimension - 1 do
                result[row, col] <- result[row, col] + term.Coefficient * pauli[row, col]

    result

let private spectralMoments (value: Complex[,]) =
    let dimension = Array2D.length1 value
    let mutable power = identity dimension

    [|
        for exponent in 1 .. dimension do
            power <- multiply power value
            yield trace power
    |]

let private oraclePath =
    Path.Combine(__SOURCE_DIRECTORY__, "..", "code", "h2_0.74_oracle.json")

if not (File.Exists(oraclePath)) then
    failwith "Missing H2 oracle. Run: make verify-data"

let private oracleDocument = JsonDocument.Parse(File.ReadAllText(oraclePath))
let private oracleRoot = oracleDocument.RootElement
let private spectra =
    oracleRoot.GetProperty("spectra_Ha_by_particle_number")

let private oracleMatrix =
    let coefficients = oracleRoot.GetProperty("coefficients_Ha")
    let result = Array2D.zeroCreate<Complex> 16 16

    for property in coefficients.EnumerateObject() do
        let pauli = signatureMatrix property.Name
        let coefficient = Complex(property.Value.GetDouble(), 0.0)
        for row in 0 .. 15 do
            for col in 0 .. 15 do
                result[row, col] <- result[row, col] + coefficient * pauli[row, col]

    result

let h2ElectronicSpectrum =
    [|
        for electronCount in 0 .. 4 do
            let values = spectra.GetProperty(string electronCount)
            for value in values.EnumerateArray() do
                yield value.GetDouble()
    |]

let assertH2JwMatrix name tolerance hamiltonian =
    let actual = hamiltonianMatrix hamiltonian
    let mutable maximumError = 0.0

    for row in 0 .. 15 do
        for col in 0 .. 15 do
            maximumError <-
                max maximumError (Complex.Abs(actual[row, col] - oracleMatrix[row, col]))

    if maximumError > tolerance then
        failwithf
            "%s dense matrix mismatch: max error %.3e exceeds %.3e"
            name
            maximumError
            tolerance

    let acceptanceAnchors = oracleRoot.GetProperty("acceptance_anchors")
    let expectedHfDiagonal =
        acceptanceAnchors.GetProperty("hf_electronic_matrix_diagonal_row_3_Ha").GetDouble()

    if abs (actual[3, 3].Real - expectedHfDiagonal) > tolerance then
        failwithf "%s HF row-3 diagonal mismatch" name

    printfn "  %-25s dense matrix passes (max error %.3e)" name maximumError

let assertH2Spectrum name tolerance hamiltonian =
    let actual = hamiltonian |> hamiltonianMatrix |> spectralMoments
    let expected =
        [|
            for exponent in 1 .. h2ElectronicSpectrum.Length ->
                h2ElectronicSpectrum |> Array.sumBy (fun value -> pown value exponent)
        |]

    let mutable maximumRelativeError = 0.0

    for index in 0 .. expected.Length - 1 do
        let scale = max 1.0 (abs expected[index])
        let relativeError =
            Complex.Abs(actual[index] - Complex(expected[index], 0.0)) / scale
        maximumRelativeError <- max maximumRelativeError relativeError

        if relativeError > tolerance then
            failwithf
                "%s spectrum moment %d mismatch: relative error %.3e exceeds %.3e"
                name
                (index + 1)
                relativeError
                tolerance

    printfn "  %-25s spectral moments pass (max relative error %.3e)" name maximumRelativeError
