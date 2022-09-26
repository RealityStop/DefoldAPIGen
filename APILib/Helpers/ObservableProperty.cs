using System.Diagnostics.Contracts;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using Splat;

namespace APILib.Helpers;

/// <summary>
///     Provides commonly required, statically-allocated, pre-canned observables.
/// </summary>
/// <typeparam name="T">
///     The observable type.
/// </typeparam>
internal static class ObservableHelper<T>
{
	/// <summary>
	///     An empty observable of type <typeparamref name="T" />.
	/// </summary>
	public static readonly IObservable<T> Empty = Observable.Empty<T>();

	/// <summary>
	///     An observable of type <typeparamref name="T" /> that never ticks a value.
	/// </summary>
	public static readonly IObservable<T> Never = Observable.Never<T>();

	/// <summary>
	///     An observable of type <typeparamref name="T" /> that ticks a single, default value.
	/// </summary>
	public static readonly IObservable<T> Default = Observable.Return(default(T));
}

/// <summary>
///     ObservableAsPropertyHelper is a class to help ViewModels implement
///     "output properties", that is, a property that is backed by an
///     Observable. The property will be read-only, but will still fire change
///     notifications. This class can be created directly, but is more often created
///     via the <see cref="OAPHCreationHelperMixin" /> extension methods.
/// </summary>
/// <typeparam name="T">The type.</typeparam>
public sealed class ObservableProperty<T> : IHandleObservableErrors, IDisposable, IEnableLogger
{
	private readonly IObservable<T> _source;
	private readonly ISubject<T> _subject;
	private readonly Lazy<ISubject<Exception>> _thrownExceptions;
	private int _activated;
	private CompositeDisposable _disposable = new();
	private T _lastValue;


	/// <summary>
	///     Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}" /> class.
	/// </summary>
	/// <param name="observable">
	///     The Observable to base the property on.
	/// </param>
	/// <param name="onChanged">
	///     The action to take when the property changes, typically this will call the
	///     ViewModel's RaisePropertyChanged method.
	/// </param>
	/// <param name="initialValue">
	///     The initial value of the property.
	/// </param>
	/// <param name="deferSubscription">
	///     A value indicating whether the <see cref="ObservableAsPropertyHelper{T}" />
	///     should defer the subscription to the <paramref name="observable" /> source
	///     until the first call to <see cref="Value" />, or if it should immediately
	///     subscribe to the the <paramref name="observable" /> source.
	/// </param>
	/// <param name="scheduler">
	///     The scheduler that the notifications will be provided on -
	///     this should normally be a Dispatcher-based scheduler.
	/// </param>
	public ObservableProperty(
		IObservable<T> observable,
		Action<T> onChanged,
		T initialValue = default,
		bool deferSubscription = false,
		IScheduler scheduler = null)
		: this(observable, onChanged, null, initialValue, deferSubscription, scheduler)
	{
	}


	/// <summary>
	///     Initializes a new instance of the <see cref="ObservableAsPropertyHelper{T}" /> class.
	/// </summary>
	/// <param name="observable">
	///     The Observable to base the property on.
	/// </param>
	/// <param name="onChanged">
	///     The action to take when the property changes, typically this will call
	///     the ViewModel's RaisePropertyChanged method.
	/// </param>
	/// <param name="onChanging">
	///     The action to take when the property changes, typically this will call
	///     the ViewModel's RaisePropertyChanging method.
	/// </param>
	/// <param name="initialValue">
	///     The initial value of the property.
	/// </param>
	/// <param name="deferSubscription">
	///     A value indicating whether the <see cref="ObservableAsPropertyHelper{T}" />
	///     should defer the subscription to the <paramref name="observable" /> source
	///     until the first call to <see cref="Value" />, or if it should immediately
	///     subscribe to the the <paramref name="observable" /> source.
	/// </param>
	/// <param name="scheduler">
	///     The scheduler that the notifications will provided on - this
	///     should normally be a Dispatcher-based scheduler.
	/// </param>
	public ObservableProperty(
		IObservable<T> observable,
		Action<T> onChanged,
		Action<T> onChanging = null,
		T initialValue = default,
		bool deferSubscription = false,
		IScheduler scheduler = null)
	{
		Contract.Requires(observable != null);
		Contract.Requires(onChanged != null);


		//scheduler = scheduler ?? CurrentThreadScheduler.Instance;
		onChanging = onChanging ?? (_ => { });

		_subject = new Subject<T>();
		IObservable<T> obs = _subject;
		if (scheduler != null)
			obs = obs.ObserveOn(scheduler);

		obs.Subscribe(
				x =>
				{
					onChanging(x);
					_lastValue = x;
					onChanged(x);
				},
				ex => _thrownExceptions.Value.OnNext(ex))
			.DisposeWith(_disposable);

		_thrownExceptions = new Lazy<ISubject<Exception>>(() =>
			new ScheduledSubject<Exception>(CurrentThreadScheduler.Instance, RxApp.DefaultExceptionHandler));

		_lastValue = initialValue;
		_source = observable.StartWith(initialValue).DistinctUntilChanged();
		if (!deferSubscription)
		{
			_source.Subscribe(_subject).DisposeWith(_disposable);
			_activated = 1;
		}
	}


	/// <summary>
	///     Gets the last provided value from the Observable.
	/// </summary>
	public T Value
	{
		get
		{
			if (Interlocked.CompareExchange(ref _activated, 1, 0) == 0)
				_source.Subscribe(_subject).DisposeWith(_disposable);

			return _lastValue;
		}
	}

	/// <summary>
	///     Gets a value indicating whether the ObservableAsPropertyHelper
	///     has subscribed to the source Observable.
	///     Useful for scenarios where you use deferred subscription and want to know if
	///     the ObservableAsPropertyHelper Value has been accessed yet.
	/// </summary>
	public bool IsSubscribed => _activated > 0;


	/// <summary>
	///     Disposes this ObservableAsPropertyHelper.
	/// </summary>
	public void Dispose()
	{
		_disposable?.Dispose();
		_disposable = null;
	}


	/// <summary>
	///     Gets an observable which signals whenever an exception would normally terminate Reactive
	///     internal state.
	/// </summary>
	public IObservable<Exception> ThrownExceptions => _thrownExceptions.Value;


	/// <summary>
	///     Constructs a "default" ObservableAsPropertyHelper object. This is
	///     useful for when you will initialize the OAPH later, but don't want
	///     bindings to access a null OAPH at startup.
	/// </summary>
	/// <param name="initialValue">
	///     The initial (and only) value of the property.
	/// </param>
	/// <param name="scheduler">
	///     The scheduler that the notifications will be provided on - this should
	///     normally be a Dispatcher-based scheduler.
	/// </param>
	/// <returns>A default property helper.</returns>
	public static ObservableProperty<T> Default(T initialValue = default, IScheduler scheduler = null)
	{
		return new ObservableProperty<T>(ObservableHelper<T>.Never, _ => { }, initialValue, false, scheduler);
	}
}