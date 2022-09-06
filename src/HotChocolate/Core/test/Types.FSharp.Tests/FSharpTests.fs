namespace HotChocolate.Types

open System
open Xunit
open Snapshooter.Xunit
open HotChocolate.Types
open HotChocolate.Resolvers
open HotChocolate.Tests
open HotChocolate
open HotChocolate.Utilities
open HotChocolate.Language
open HotChocolate.Execution
open Microsoft.Extensions.DependencyInjection

type Foo = {
    Bar: int
}

type FooInputType() =
    inherit InputObjectType<Foo>()

type FooType() =
    inherit ObjectType<Foo>()

type QueryClass() =
    member this.Scalars (values: list<int>) = values
    member this.Objects (values: list<Foo>) = values

type Query() =
    inherit ObjectType<obj>()
    with
        override self.Configure(descriptor: IObjectTypeDescriptor<obj>) =
            descriptor.Field("scalars")
                .Argument("values", fun a -> a.Type<ListType<IntType>>() |> ignore)
                .Resolve<list<int>>(fun (context: IResolverContext) -> context.ArgumentValue<list<int>>("values"))
                .Type<ListType<IntType>>()
            |> ignore
            
            descriptor.Field("objects")
                .Argument("values", fun a -> a.Type<ListType<FooInputType>>() |> ignore)
                .Resolve<list<Foo>>(fun (context: IResolverContext) -> context.ArgumentValue<list<Foo>>("values"))
                .Type<ListType<FooType>>()
            |> ignore

type QueryRecord = {
    Scalars: list<int>
    Foo: list<Foo>
}

module FSharpTests =
    [<Fact>]
    let Integration_List_ListValues_Scalars () =
        // arrange
        let executor =
            ServiceCollection()
                .AddGraphQL()
                .AddQueryType<Query>()
                .BuildRequestExecutorAsync()
                .AsTask()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // act
        let result =
            executor.ExecuteAsync(
                QueryRequestBuilder
                    .New()
                    .SetQuery("{ scalars(values: [1,2]) }")
                    .Create()
            )
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // assert
        SnapshotExtensions.MatchSnapshot(result)

    [<Fact>]
    let Integration_List_ScalarValue_Scalars () =
        // arrange
        let executor =
            ServiceCollection()
                .AddGraphQL()
                .AddQueryType<Query>()
                .BuildRequestExecutorAsync()
                .AsTask()
            |> Async.AwaitTask
            |> Async.RunSynchronously
        
        // act
        let result =
            executor.ExecuteAsync(
                QueryRequestBuilder
                    .New()
                    .SetQuery("{ scalars(values: 1) }")
                    .Create()
            )
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // assert
        SnapshotExtensions.MatchSnapshotAsync(result)

    [<Fact>]
    let Integration_List_ListValues_Object () =
        // arrange
        let executor =
            ServiceCollection()
                .AddGraphQL()
                .AddQueryType<Query>()
                .BuildRequestExecutorAsync()
                .AsTask()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // act
        let result =
            executor.ExecuteAsync(
                QueryRequestBuilder
                    .New()
                    .SetQuery("{ objects(values: [{ bar: 1 }, { bar: 2 }]) { bar } }")
                    .Create()
            )
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // assert
        SnapshotExtensions.MatchSnapshotAsync(result)

    [<Fact>]
    let Integration_List_ScalarValue_Object () =
        // arrange
        let executor =
            ServiceCollection()
                .AddGraphQL()
                .AddQueryType<Query>()
                .BuildRequestExecutorAsync()
                .AsTask()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // act
        let result =
            executor.ExecuteAsync(
                QueryRequestBuilder
                    .New()
                    .SetQuery("{ objects(values: { bar: 1 }) { bar } }")
                    .Create()
            )
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // assert
        SnapshotExtensions.MatchSnapshotAsync(result)


    [<Fact>]
    let Integration_List_ListValues_Scalars_Annotation () =
        // arrange
        let executor =
            ServiceCollection()
                .AddGraphQL()
                .AddQueryType<QueryClass>()
                .BuildRequestExecutorAsync()
                .AsTask()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // act
        let result =
            executor.ExecuteAsync(
                QueryRequestBuilder
                    .New()
                    .SetQuery("{ scalars(values: [1,2]) }")
                    .Create()
            )
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // assert
        SnapshotExtensions.MatchSnapshot(result)

    [<Fact>]
    let Integration_List_ScalarValue_Scalars_Annotation () =
        // arrange
        let executor =
            ServiceCollection()
                .AddGraphQL()
                .AddQueryType<QueryClass>()
                .BuildRequestExecutorAsync()
                .AsTask()
            |> Async.AwaitTask
            |> Async.RunSynchronously
        
        // act
        let result =
            executor.ExecuteAsync(
                QueryRequestBuilder
                    .New()
                    .SetQuery("{ scalars(values: 1) }")
                    .Create()
            )
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // assert
        SnapshotExtensions.MatchSnapshotAsync(result)

    [<Fact>]
    let Integration_List_ListValues_Object_Annotation () =
        // arrange
        let executor =
            ServiceCollection()
                .AddGraphQL()
                .AddQueryType<QueryClass>()
                .BuildRequestExecutorAsync()
                .AsTask()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // act
        let result =
            executor.ExecuteAsync(
                QueryRequestBuilder
                    .New()
                    .SetQuery("{ objects(values: [{ bar: 1 }, { bar: 2 }]) { bar } }")
                    .Create()
            )
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // assert
        SnapshotExtensions.MatchSnapshotAsync(result)

    [<Fact>]
    let Integration_List_ScalarValue_Object_Annotation () =
        // arrange
        let executor =
            ServiceCollection()
                .AddGraphQL()
                .AddQueryType<QueryClass>()
                .BuildRequestExecutorAsync()
                .AsTask()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // act
        let result =
            executor.ExecuteAsync(
                QueryRequestBuilder
                    .New()
                    .SetQuery("{ objects(values: { bar: 1 }) { bar } }")
                    .Create()
            )
            |> Async.AwaitTask
            |> Async.RunSynchronously

        // assert
        SnapshotExtensions.MatchSnapshotAsync(result)
