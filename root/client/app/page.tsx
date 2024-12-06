'use client';

import { RadarChartComponent } from '@/components/charts/radarchart';
import { AreaChartComponent } from '@/components/charts/areachart';
import { LineChartComponent } from '@/components/charts/barchart';
import InteractiveBarChart from '@/components/charts/linechart';
import { useEffect, useState } from 'react';
import identityService from '@/services/identity-service';
import { useRouter } from 'next/navigation';

export default function Home() {
  const [fullName, setFullName] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    let mounted = true;

    const checkAuth = async () => {
      try {
        const token = localStorage.getItem('authToken');
        if (!token) {
          if (mounted) setIsLoading(false);
          return;
        }

        const user = await identityService.getCurrentUser();
        if (mounted && user) {
          setFullName(user.fullName);
        }
      } catch (error) {
        console.error('Error fetching user:', error);
      } finally {
        if (mounted) setIsLoading(false);
      }
    };

    checkAuth();

    return () => {
      mounted = false;
    };
  }, []);

  const handleLogout = async () => {
    await identityService.logout();
    router.push('/auth/login');
  };

  if (isLoading) {
    return null;
  }

  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-24 bg-indigo-50">
      <h1 className="mb-8 text-4xl font-bold text-indigo-900">
        {fullName ? (
          <span>Welcome, {fullName}</span>
        ) : (
          'Join us'
        )}
      </h1>
      <div className="w-full max-w-7xl space-y-8">
        <div className="p-6 bg-white rounded-lg shadow-lg">
          <InteractiveBarChart />
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          <div className="p-6 bg-white rounded-lg shadow-lg">
            <RadarChartComponent />
          </div>
          <div className="p-6 bg-white rounded-lg shadow-lg">
            <LineChartComponent />
          </div>
          <div className="p-6 bg-white rounded-lg shadow-lg">
            <AreaChartComponent />
          </div>
        </div>
      </div>
    </main>
  );
}

