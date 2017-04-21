﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditBot
{
    class TooLowCapacityException : Exception
    {
        public TooLowCapacityException(string message)
            : base(message)
        {

        }
    }
    class TokenBucket
    {
        private int _tokens;
        private int _capacity;
        private DateTime _time;
        private int _rate;
        /// <summary>
        /// A bucket for your tokens.
        /// </summary>
        /// <param name="capacity">The starting amount of tokens in the bucket</param>
        /// <param name="rate">The timespan in which tokens may be removed in the bucket</param>
        /// <exception cref="TooLowCapacityException">If Capacity is too low to start, => 0</exception>
        public TokenBucket(int capacity, int rate)
        {
            if (capacity <= 0)
            {
                throw new TooLowCapacityException("Your capacity is too low");
            }
            _capacity = capacity;
            _tokens = capacity;
            _rate = rate;
            _time = DateTime.Now;
        }

        public bool RequestIsAllowed()
        {
            if (_tokens > 0)
            {
                _tokens -= 1;
                return true;
            }
            else
            {
                if (Refill())
                {
                    _tokens -= 1;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        private bool Refill()
        {
            DateTime now = DateTime.Now;
            double difference = (now - _time).TotalSeconds;
            if (difference >= _rate)
            {
                _time = DateTime.Now;
                _tokens = _capacity;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int TimeUntilRefresh()
        {
            return _rate - (DateTime.Now - _time).Seconds;
        }
    }
}